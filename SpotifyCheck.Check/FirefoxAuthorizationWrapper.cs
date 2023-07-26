using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using SpotifyCheck.Check.Configurations;
using SpotifyCheck.Check.Exceptions;
using Cookie = System.Net.Cookie;
using InvalidDataException = SpotifyCheck.Check.Exceptions.InvalidDataException;
using Proxy = SpotifyCheck.Core.Models.Proxy;

namespace SpotifyCheck.Check;

public class FirefoxAuthorizationWrapper : IBrowserAuthorizationWrapper
{
    private readonly BrowserOptions _browserOptions;
    private readonly ILogger<FirefoxAuthorizationWrapper> _logger;
    private FirefoxDriver _driver;

    public FirefoxAuthorizationWrapper(IOptions<BrowserOptions> browserOptions, ILogger<FirefoxAuthorizationWrapper> logger)
    {
        _logger = logger;
        _browserOptions = browserOptions.Value;
    }

    public void InitBrowser(Guid taskId)
    {
        var firefox = new FirefoxProfile();
        var options = new FirefoxOptions { Profile = firefox };
        if (_browserOptions.LaunchHeadless) options.AddArgument("-headless");
        options.PageLoadStrategy = PageLoadStrategy.Eager;
        _driver = new FirefoxDriver(options);
        _driver.InstallAddOnFromFile(_browserOptions.ProxyExtensionPath);
        _logger.LogTrace("{TaskId} | Browser init", taskId);
    }

    public void SetProxy(Guid taskId, Proxy? proxy)
    {
        if (proxy == null) return;
        _logger.LogTrace("{TaskId} | Set proxy", taskId);
        var handles = _driver.WindowHandles;
        var dateTimeNowTmp = DateTime.Now;

        while (handles.Count < 2)
        {
            if (DateTime.Now - dateTimeNowTmp > TimeSpan.FromMilliseconds(_browserOptions.GetProxyExtensionWindowsTimeoutMs))
                throw new Exception("GetProxyExtensionUrlTimeoutMs timeout");

            handles = _driver.WindowHandles;
            Thread.Sleep(10);
        }

        _logger.LogTrace("{TaskId} | Windows received", taskId);
        _driver.SwitchTo().Window(_driver.WindowHandles[1]);
        dateTimeNowTmp = DateTime.Now;

        while (_driver.Url.Split('/').Length < 2)
        {
            if (DateTime.Now - dateTimeNowTmp > TimeSpan.FromMilliseconds(_browserOptions.GetProxyExtensionUrlTimeoutMs))
                throw new Exception($"GetProxyExtensionUrlTimeoutMs timeout. Url was: {_driver.Url}");

            Thread.Sleep(10);
        }

        _logger.LogTrace("{TaskId} | Extension url received", taskId);
        _driver.Navigate().GoToUrl($"moz-extension://{_driver.Url.Split('/')[2]}/proxy.html");
        _logger.LogTrace("{TaskId} | Set proxy data", taskId);
        var proxyFormWait = new WebDriverWait(_driver, TimeSpan.FromMilliseconds(_browserOptions.GetProxyFormTimeoutMs));
        var proxyIp = proxyFormWait.Until(_driver => _driver.FindElement(By.CssSelector("#proxyAddress")));
        var proxyPort = proxyFormWait.Until(_driver => _driver.FindElement(By.CssSelector("#proxyPort")));
        var proxyLogin = proxyFormWait.Until(_driver => _driver.FindElement(By.CssSelector("#proxyUsername")));
        var proxyPassword = proxyFormWait.Until(_driver => _driver.FindElement(By.CssSelector("#proxyPassword")));
        var proxyType = proxyFormWait.Until(_driver => _driver.FindElement(By.CssSelector("#proxyType")));
        var proxySubmit = proxyFormWait.Until(_driver => _driver.FindElement(By.CssSelector("button[type=\"submit\"]")));
        proxyIp.SendKeys(proxy.Address);
        proxyPort.SendKeys(proxy.Port);
        proxyLogin.SendKeys(proxy.Login);
        proxyPassword.SendKeys(proxy.Password);
        var selectElement = new SelectElement(proxyType);
        selectElement.SelectByIndex((int)proxy.Type);
        proxySubmit.Click();

        var loaderWait =
            new WebDriverWait(_driver, TimeSpan.FromMilliseconds(_browserOptions.GetProxyLoaderTimeoutMs))
            {
                PollingInterval = TimeSpan.FromMilliseconds(10)
            };

        loaderWait.IgnoreExceptionTypes(typeof(NoSuchElementException));

        var spinner = loaderWait.Until(
            _driver =>
            {
                // Need because selenium throw exception at few cases
                try
                {
                    return _driver.FindElement(By.CssSelector(".spinner"));
                }
                catch (NoSuchElementException e)
                {
                    return null;
                }
            }
        );

        while (spinner != null && spinner?.GetAttribute("style") != "display: none;") Thread.Sleep(10);

        var proxyChange =
            new WebDriverWait(_driver, TimeSpan.FromMilliseconds(_browserOptions.GetProxySelectTimeoutMs)).Until(
                _driver => _driver.FindElement(By.CssSelector("#selectAndSync select"))
            );

        var selectProxy = new SelectElement(proxyChange);
        selectProxy.SelectByIndex(selectProxy.Options.Count - 1);
        _logger.LogTrace("{TaskId} | Proxy set", taskId);
    }

    public ReadOnlyCollection<Cookie>? Login(Guid taskId, string login, string password)
    {
        _logger.LogTrace("{TaskId} | Go to spotify url", taskId);
        _driver.Navigate().GoToUrl(_browserOptions.SpotifyUrl);

        if (_driver.PageSource.Contains("Too Many Requests"))
        {
            _logger.LogDebug("{TaskId} | Spotify: Too Many Requests", taskId);
            throw new ChangeProxyException();
        }

        if (_driver.PageSource.Contains("403 Forbidden"))
        {
            _logger.LogDebug("{TaskId} | Spotify: 403 Forbidden", taskId);
            throw new ChangeProxyException();
        }

        var formWait = new WebDriverWait(_driver, TimeSpan.FromMilliseconds(_browserOptions.GetSpotifyFormTimeoutMs));
        var loginInput = formWait.Until(_driver => _driver.FindElement(By.CssSelector("#login-username")));
        var passwordInput = formWait.Until(_driver => _driver.FindElement(By.CssSelector("#login-password")));
        var loginButton = formWait.Until(_driver => _driver.FindElement(By.CssSelector("#login-button")));
        loginInput.SendKeys(login);
        passwordInput.SendKeys(password);
        loginButton.Click();
        _logger.LogTrace("{TaskId} | Try login", taskId);

        var errorMessageWait = new WebDriverWait(_driver, TimeSpan.FromMilliseconds(_browserOptions.GetSpotifyErrorMessageMs))
        {
            PollingInterval = TimeSpan.FromMilliseconds(10)
        };

        errorMessageWait.IgnoreExceptionTypes(typeof(NoSuchElementException));
        ReadOnlyCollection<OpenQA.Selenium.Cookie>? cookies;
        var tmpDateTime = DateTime.Now;

        do
        {
            var wrongLogin =
                errorMessageWait.Until(drv => drv.FindElement(By.CssSelector("div[data-encore-id=\"banner\" ] span")));

            if (DateTime.Now - tmpDateTime > TimeSpan.FromMilliseconds(_browserOptions.GetSpotifyCookieTimeoutMs))
            {
                if (wrongLogin.Text.Contains("Oops! Something went wrong"))
                {
                    _logger.LogDebug("{TaskId} | Spotify: Oops! Something went wrong", taskId);
                    throw new ChangeProxyException();
                }

                throw new UnknownException(_driver.PageSource, wrongLogin?.Text ?? string.Empty);
            }

            if (wrongLogin is { Displayed: true, Enabled: true })
            {
                if (wrongLogin.Text.Contains("Oops! Something went wrong"))
                {
                    Thread.Sleep(50);
                    loginButton.Click();
                }
                else if (wrongLogin.Text.Contains("Incorrect username or password"))
                {
                    Thread.Sleep(50);
                    throw new InvalidDataException();
                }
                else
                {
                    throw new UnknownException(_driver.PageSource, wrongLogin?.Text ?? string.Empty);
                }
            }

            cookies = _driver.Manage().Cookies.AllCookies;
            Thread.Sleep(10);
        } while (!cookies.Any(x => x.Name.Contains("sp_dc")));

        _logger.LogTrace("{TaskId} | Cookie received", taskId);
        var result = new List<Cookie>();
        foreach (var t in cookies) result.Add(new Cookie(t.Name, t.Value, t.Path, t.Domain));
        return result.AsReadOnly();
    }

    public void Dispose()
    {
        _driver?.Quit();
        _driver?.Dispose();
    }
}