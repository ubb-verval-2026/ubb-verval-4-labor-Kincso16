using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using FluentAssertions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace DatesAndStuff.Web.Tests;

[TestFixture]
public class PersonPageTests
{
    private IWebDriver driver;
    private StringBuilder verificationErrors;
    private const string BaseURL = "http://localhost:5091";
    private bool acceptNextAlert = true;

    private Process? _blazorProcess;

    [OneTimeSetUp]
    public void StartBlazorServer()
    {
        var webProjectPath = Path.GetFullPath(Path.Combine(
            Assembly.GetExecutingAssembly().Location,
            "../../../../../../src/DatesAndStuff.Web/DatesAndStuff.Web.csproj"
            ));

        var webProjFolderPath = Path.GetDirectoryName(webProjectPath);

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{webProjectPath}\"",
            //Arguments = "dotnet run --no-build",
            WorkingDirectory = webProjFolderPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        _blazorProcess = Process.Start(startInfo);

        // Wait for the app to become available
        var client = new HttpClient();
        var timeout = TimeSpan.FromSeconds(30);
        var start = DateTime.Now;

        while (DateTime.Now - start < timeout)
        {
            try
            {
                var result = client.GetAsync(BaseURL).Result;
                if (result.IsSuccessStatusCode)
                {
                    break;
                }
            }
            catch (Exception e)
            {
                Thread.Sleep(1000);
            }
        }
    }

    [OneTimeTearDown]
    public void StopBlazorServer()
    {
        if (_blazorProcess != null && !_blazorProcess.HasExited)
        {
            _blazorProcess.Kill(true);
            _blazorProcess.Dispose();
        }
    }

    [SetUp]
    public void SetupTest()
    {
        driver = new ChromeDriver();
        verificationErrors = new StringBuilder();
    }

    [TearDown]
    public void TeardownTest()
    {
        try
        {
            driver.Quit();
            driver.Dispose();
        }
        catch (Exception)
        {
            // Ignore errors if unable to close the browser
        }
        Assert.That(verificationErrors.ToString(), Is.EqualTo(""));
    }

    [Test]
    [TestCase(0, 5000)]
    [TestCase(5, 5250)]
    [TestCase(10, 5500)]
    [TestCase(20, 6000)]
    public void Person_SalaryIncrease_ShouldIncrease(double percentage, double expectedSalary)
    {
        driver.Navigate().GoToUrl(BaseURL);

        // Wait for the app to load
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

        // Navigate to Person page
        wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@data-test='PersonPageNavigation']"))).Click();

        // Wait for page to stabilize
        wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@data-test='SalaryIncreasePercentageInput']")));

        var inputLocator = By.XPath("//*[@data-test='SalaryIncreasePercentageInput']");

        // Clear input
        wait.Until(ExpectedConditions.ElementIsVisible(inputLocator)).Clear();

        // Type percentage
        wait.Until(ExpectedConditions.ElementIsVisible(inputLocator))
            .SendKeys(percentage.ToString());

        // Submit
        wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@data-test='SalaryIncreaseSubmitButton']"))).Click();

        // Wait for updated salary to appear
        var salaryLabel = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@data-test='DisplayedSalary']")));

        var salaryAfterSubmission = double.Parse(salaryLabel.Text);

        salaryAfterSubmission.Should().BeApproximately(expectedSalary, 0.001);
    }

    [Test]
    [TestCase(-20)]
    [TestCase(-10)]
    [TestCase(-11)]
    public void Person_SalaryIncrease_ShouldNotUpdate_WhenPercentageBelowMinusTen(double percentage)
    {
        driver.Navigate().GoToUrl(BaseURL);

        // Wait for the app to load
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

        // Navigate to Person page
        wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@data-test='PersonPageNavigation']"))).Click();

        // Wait for the page to load
        var inputLocator = By.XPath("//*[@data-test='SalaryIncreasePercentageInput']");
        wait.Until(ExpectedConditions.ElementIsVisible(inputLocator));

        // Enter invalid value
        var input = wait.Until(ExpectedConditions.ElementIsVisible(inputLocator));
        input.Clear();
        input.SendKeys(percentage.ToString());

        // Submit
        wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@data-test='SalaryIncreaseSubmitButton']"))).Click();

        var salaryLabel = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@data-test='DisplayedSalary']")));
        double salary = double.Parse(salaryLabel.Text);

        salary.Should().Be(5000);
        /*
        // 1) Validation summary (top of page)
        var summaryError = wait.Until(ExpectedConditions.ElementIsVisible(
            By.CssSelector("li.validation-message")));
        summaryError.Text.Should().Contain("between -10");

        // 2) Field-level error (under the input)
        var fieldError = wait.Until(ExpectedConditions.ElementIsVisible(
            By.CssSelector("div.validation-message")));
        fieldError.Text.Should().Contain("between -10");
        */
    }


    private bool IsElementPresent(By by)
    {
        try
        {
            driver.FindElement(by);
            return true;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    private bool IsAlertPresent()
    {
        try
        {
            driver.SwitchTo().Alert();
            return true;
        }
        catch (NoAlertPresentException)
        {
            return false;
        }
    }

    private string CloseAlertAndGetItsText()
    {
        try
        {
            IAlert alert = driver.SwitchTo().Alert();
            string alertText = alert.Text;
            if (acceptNextAlert)
            {
                alert.Accept();
            }
            else
            {
                alert.Dismiss();
            }
            return alertText;
        }
        finally
        {
            acceptNextAlert = true;
        }
    }
}