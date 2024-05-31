using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;

namespace UnitTest;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [AvaloniaTest]
    public void Should_Type_Text_Into_TextBox()
    {
        // Setup controls:
        var textBox = new TextBox();
        var window = new Window { Content = textBox };

        // Open window:
        window.Show();
 
        // Focus text box:
        textBox.Focus();

        // Simulate text input:
        window.KeyTextInput("Hello World");

        // Assert:
        Assert.That(textBox.Text, Is.EqualTo("Hello World"));
    }
}