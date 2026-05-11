using System.Windows;
using System.Windows.Input;
using WpfApp4.ViewModels;

namespace WpfApp4;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new GuessNumberViewModel();
        Loaded += (_, _) => GuessInputBox.Focus();
    }

    private void GuessTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = ContainsNonDigit(e.Text);
    }

    private void GuessTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (!e.SourceDataObject.GetDataPresent(DataFormats.Text, true))
        {
            e.CancelCommand();
            return;
        }

        string pastedText = e.SourceDataObject.GetData(DataFormats.Text) as string ?? string.Empty;
        if (ContainsNonDigit(pastedText))
        {
            e.CancelCommand();
        }
    }

    private static bool ContainsNonDigit(string text)
    {
        return text.Any(character => !char.IsDigit(character));
    }
}
