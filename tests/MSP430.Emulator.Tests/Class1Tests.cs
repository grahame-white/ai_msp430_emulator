namespace MSP430.Emulator.Tests;

public class Class1Tests
{
    [Fact]
    public void Test1()
    {
        // TODO: Add unit tests for MSP430 emulator components
        // This is a placeholder test for the initial project setup
        Assert.True(true);
    }

    [Fact]
    public void Class1_GetMessage_ReturnsExpectedMessage()
    {
        var class1 = new Class1();
        string result = class1.GetMessage();
        Assert.Equal("Hello, MSP430 Emulator!", result);
    }

    [Fact]
    public void Class1_Add_ReturnsSum()
    {
        var class1 = new Class1();
        int result = class1.Add(2, 3);
        Assert.Equal(5, result);
    }
}
