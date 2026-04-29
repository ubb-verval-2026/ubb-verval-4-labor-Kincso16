using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using FluentAssertions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

[TestFixture]
public class BlazeDemoTests
{
    private ChromeDriver driver;
    private WebDriverWait wait;

    [SetUp]
    public void Setup()
    {
        driver = new ChromeDriver();
        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
    }

    [TearDown]
    public void Teardown()
    {
        driver.Dispose();
    }

    [Test]
    public void BlazeDemo_ShouldHaveAtLeastThreeFlights_MexicoCity_To_Dublin()
    {
        driver.Navigate().GoToUrl("https://blazedemo.com");

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

        // Select departure city
        var fromSelect = wait.Until(ExpectedConditions.ElementIsVisible(By.Name("fromPort")));
        fromSelect.Click();
        fromSelect.FindElement(By.XPath("//option[text()='Mexico City']")).Click();

        // Select destination city
        var toSelect = wait.Until(ExpectedConditions.ElementIsVisible(By.Name("toPort")));
        toSelect.Click();
        toSelect.FindElement(By.XPath("//option[text()='Dublin']")).Click();

        // Submit
        wait.Until(ExpectedConditions.ElementIsVisible(
            By.CssSelector("input[type='submit']"))).Click();

        // Wait for results table
        var table = wait.Until(ExpectedConditions.ElementIsVisible(
            By.CssSelector("table.table")));

        // Count rows
        var rows = table.FindElements(By.CssSelector("tbody tr"));

        rows.Count.Should().BeGreaterThanOrEqualTo(3);
    }

}
