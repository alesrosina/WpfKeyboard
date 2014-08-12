using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;

namespace WpfKeyboard
{

    /// <summary>
    /// Interaction logic for KeyBoardControl.xaml
    /// </summary>
    public partial class KeyBoardControl : UserControl
    {
        private bool _shiftUp = true;
        private bool _capsLock = false;
        private bool _symbolUp = false;

        public KeyBoardControl()
        {
            InitializeComponent();
            Background = new SolidColorBrush(Color.FromRgb(52, 57, 60));
            KeyBackColor = new SolidColorBrush(Color.FromRgb(82, 87, 90));
            KeyDarkerBackColor = new SolidColorBrush(Color.FromRgb(67, 72, 75));
            KeyEnterColor = new SolidColorBrush(Color.FromRgb(67, 72, 75));

            if (!DesignerProperties.GetIsInDesignMode(this))
                LoadCharData();

            Visibility = Visibility.Collapsed;

            IsEnabledChanged += KeyBoardControl_IsEnabledChanged;
            Loaded += KeyBoardControl_Loaded;
            InputLanguageManager.Current.InputLanguageChanged += Current_InputLanguageChanged;

            ShiftUp = false;
        }

        void Current_InputLanguageChanged(object sender, InputLanguageEventArgs e)
        {
            LoadCharData();
            ShiftAction(ShiftUp, _capsLock);
            //throw new NotImplementedException();
        }

        private void KeyBoardControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsEnabled)
                Keyboard.AddGotKeyboardFocusHandler(Parent, GotKeyBoardHandler);
        }

        private void KeyBoardControl_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Parent != null)
            {
                if (IsEnabled)
                    Keyboard.AddGotKeyboardFocusHandler(Parent, GotKeyBoardHandler);
                else
                    Keyboard.RemoveGotKeyboardFocusHandler(Parent, GotKeyBoardHandler);
            }
        }

        private void GotKeyBoardHandler(object sender, KeyboardFocusChangedEventArgs e)
        {
            var txtInput = e.NewFocus as TextBox;
            if (txtInput != null)
            {

                if (txtInput.InputScope == null)
                {
                    CharsKeyboardTab.IsSelected = true;
                }
                else
                {
                    if (txtInput.InputScope.Names.Count > 0)
                    {
                        var scope = txtInput.InputScope.Names[0] as InputScopeName;
                        //scope.NameValue

                        if (scope != null)
                        {
                            if (scope.NameValue == InputScopeNameValue.Number)
                            {
                                NumKeyboardTab.IsSelected = true;
                            }
                        }
                    }

                }
            }

            Visibility = e.NewFocus.GetType() == typeof (TextBox) ? Visibility.Visible : Visibility.Collapsed;
        }


        #region Color Properties

        [Bindable(true)]
        [Category("Brush")]
        public Brush KeyBackColor
        {
            get { return (Brush) GetValue(KeyBackColorProperty); }
            set { SetValue(KeyBackColorProperty, value); }
        }

        public static readonly DependencyProperty KeyBackColorProperty =
            DependencyProperty.Register("KeyBackColor", typeof (Brush), typeof (KeyBoardControl),
                new PropertyMetadata(null));

        [Bindable(true)]
        [Category("Brush")]
        public Brush KeyEnterColor
        {
            get { return (Brush) GetValue(KeyEnterColorProperty); }
            set { SetValue(KeyEnterColorProperty, value); }
        }

        public static readonly DependencyProperty KeyEnterColorProperty =
            DependencyProperty.Register("KeyEnterColor", typeof (Brush), typeof (KeyBoardControl),
                new PropertyMetadata(null));



        [Bindable(true)]
        [Category("Brush")]
        public Brush KeyDarkerBackColor
        {
            get { return (Brush) GetValue(KeyDarkerBackColorProperty); }
            set { SetValue(KeyDarkerBackColorProperty, value); }
        }

        public static readonly DependencyProperty KeyDarkerBackColorProperty =
            DependencyProperty.Register("KeyDarkerBackColor", typeof (Brush), typeof (KeyBoardControl),
                new PropertyMetadata(null));

        #endregion


        private bool ShiftUp
        {
            get { return _shiftUp; }
            set
            {
                if (value != _shiftUp)
                {

                    if (_capsLock != true)
                    {
                        ShiftAction(value, _symbolUp);
                        _shiftUp = value;
                    }
                    else
                    {
                        ShiftAction(true, _symbolUp);
                    }

                }

            }
        }

        private void LetterKey_Click(object sender, RoutedEventArgs e)
        {

            var tmpbtn = sender as Button;
            if (tmpbtn == null)
                return;

            var target = Keyboard.FocusedElement;
            var routedEvent = TextCompositionManager.TextInputEvent;

            target.RaiseEvent(
                new TextCompositionEventArgs(
                    InputManager.Current.PrimaryKeyboardDevice,
                    new TextComposition(InputManager.Current, target, tmpbtn.Content.ToString()))
                {
                    RoutedEvent = routedEvent
                });

            ShiftUp = false;

            //some fishy stuff for animations ...
            //Task.Run(() =>
            //{
            //    Thread.Sleep(1000);
            //    this.Dispatcher.InvokeAsync(() => VisualStateManager.GoToState(sender as FrameworkElement, "Normal", true));
            //});
        }

        private void CommandKey_Click(object sender, RoutedEventArgs e)
        {


            var tmpbtn = sender as Button;
            if (tmpbtn == null)
                return;
            if (tmpbtn.CommandParameter == null)
                return;

            switch (tmpbtn.CommandParameter.ToString())
            {
                case "enter":
                    EditingCommands.EnterLineBreak.Execute(null, Keyboard.FocusedElement);
                    break;
                case "backspace":
                    EditingCommands.Backspace.Execute(null, Keyboard.FocusedElement);
                    break;
                case "left":
                    EditingCommands.MoveLeftByCharacter.Execute(null, Keyboard.FocusedElement);
                    break;
                case "right":
                    EditingCommands.MoveRightByCharacter.Execute(null, Keyboard.FocusedElement);
                    break;
                case "hide":
                    Visibility = Visibility.Collapsed;
                    Keyboard.ClearFocus();
                    break;
                case "symbol":
                    _symbolUp = !_symbolUp;

                    if (!ShiftUp)
                        ShiftAction(ShiftUp, _symbolUp);
                    ShiftUp = false;
                    break;
            }


        }

        private void CommandShift_Click(object sender, RoutedEventArgs e)
        {
            _capsLock = false;
            ShiftUp = !ShiftUp;

        }

        private void CommandShift_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //DateTime.Now.
            e.Handled = true;
            _capsLock = true;
            ShiftAction(true, _symbolUp);
        }

        private void ShiftAction(bool shiftUp, bool symbolUp)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(CharsKeyBoard); i++)
            {
                var stackpanel = VisualTreeHelper.GetChild(CharsKeyBoard, i);
                if (stackpanel != null)
                {
                    for (int k = 0; k < VisualTreeHelper.GetChildrenCount(stackpanel); k++)
                    {
                        var button = VisualTreeHelper.GetChild(stackpanel, k) as Button;

                        if (button == null)
                            continue;

                        if (button.Tag != null)
                        {
                            var key = button.Tag as KeyboardKey;
                            if (key == null)
                                continue;
                            if (shiftUp && symbolUp)
                                button.Content = key.SymbolValueUpper.ToString();
                            else if (!shiftUp && symbolUp)
                                button.Content = key.SymbolValueLower.ToString();
                            else if (shiftUp && !symbolUp)
                                button.Content = key.CharValueUpper.ToString();
                            else if (!shiftUp && !symbolUp)
                                button.Content = key.CharValueLower.ToString();
                        }
                    }
                }
            }
        }



        private void LoadCharData()
        {


            var lines2 = new List<KeyboardKey>();

            var serializer = new XmlSerializer(lines2.GetType());
            var currentPath = Path.Combine(Directory.GetCurrentDirectory(), "keyboard");

            if (!Directory.Exists(currentPath))
                Directory.CreateDirectory(currentPath);

            //var files = Directory.GetFiles(currentPath);


            var fileName = InputLanguageManager.Current.CurrentInputLanguage.Name + ".keys";
            if (!File.Exists(Path.Combine(currentPath, fileName)))
            {
                fileName = "en-US.keys";
                if (!File.Exists(Path.Combine(currentPath, fileName)))
                {
                    FillDefaultEnUsKeyboard(lines2);
                    using (var writer = new StreamWriter(Path.Combine(currentPath, fileName)))
                    {
                        serializer.Serialize(writer, lines2);
                    }
                }
            }



            using (var fs = File.Open(Path.Combine(currentPath, fileName), FileMode.Open))
            {
                lines2 = serializer.Deserialize(fs) as List<KeyboardKey>;
            }


            int count = 0;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(CharsKeyBoard); i++)
            {
                var stackpanel = VisualTreeHelper.GetChild(CharsKeyBoard, i);
                if (stackpanel != null)
                {
                    for (int k = 0; k < VisualTreeHelper.GetChildrenCount(stackpanel); k++)
                    {
                        var button = VisualTreeHelper.GetChild(stackpanel, k) as Button;

                        if (button == null)
                            continue;

                        if (!String.IsNullOrEmpty(button.Name))
                        {
                            button.Tag = lines2[count];
                            count++;
                        }
                    }
                }
            }

        }

        private static void FillDefaultEnUsKeyboard(List<KeyboardKey> lines2)
        {

            //lines2.Add(new KeyboardKey() { CharValueLower = 'љ', CharValueUpper = 'Љ', SymbolValueLower = '1', SymbolValueUpper = '1' });
            //lines2.Add(new KeyboardKey() { CharValueLower = 'њ', CharValueUpper = 'Њ', SymbolValueLower = '2', SymbolValueUpper = '2' });
            //lines2.Add(new KeyboardKey() { CharValueLower = 'е', CharValueUpper = 'Е', SymbolValueLower = '3', SymbolValueUpper = '3' });
            //lines2.Add(new KeyboardKey() { CharValueLower = 'р', CharValueUpper = 'Р', SymbolValueLower = '4', SymbolValueUpper = '4' });
            //lines2.Add(new KeyboardKey() { CharValueLower = 'т', CharValueUpper = 'Т', SymbolValueLower = '5', SymbolValueUpper = '5' });
            //lines2.Add(new KeyboardKey() { CharValueLower = 'з', CharValueUpper = 'З', SymbolValueLower = '6', SymbolValueUpper = '6' });
            //lines2.Add(new KeyboardKey() { CharValueLower = 'у', CharValueUpper = 'У', SymbolValueLower = '7', SymbolValueUpper = '7' });
            //lines2.Add(new KeyboardKey() { CharValueLower = 'и', CharValueUpper = 'И', SymbolValueLower = '8', SymbolValueUpper = '8' });
            //lines2.Add(new KeyboardKey() { CharValueLower = 'о', CharValueUpper = 'О', SymbolValueLower = '9', SymbolValueUpper = '9' });
            //lines2.Add(new KeyboardKey() { CharValueLower = 'п', CharValueUpper = 'П', SymbolValueLower = '0', SymbolValueUpper = '0' });
            //lines2.Add(new KeyboardKey() { CharValueLower = 'ш', CharValueUpper = 'Ш', SymbolValueLower = '\'', SymbolValueUpper = '?' });
            //lines2.Add(new KeyboardKey() { CharValueLower = 'ђ', CharValueUpper = 'Ђ', SymbolValueLower = '+', SymbolValueUpper = '*' });

            //lines2.Add(new KeyboardKey() { CharValueLower = 'а', CharValueUpper = 'А', SymbolValueLower = '!', SymbolValueUpper = '!' });
            //lines2.Add(new KeyboardKey() { CharValueLower = 'с', CharValueUpper = 'С', SymbolValueLower = '"', SymbolValueUpper = '"' });
            //lines2.Add(new KeyboardKey() { CharValueLower = 'д', CharValueUpper = 'Д', SymbolValueLower = '#', SymbolValueUpper = '#' });
            //lines2.Add(new KeyboardKey() { CharValueLower = 'ф', CharValueUpper = 'Ф', SymbolValueLower = '$', SymbolValueUpper = '$' });
            //lines2.Add(new KeyboardKey() { CharValueLower = 'г', CharValueUpper = 'Г', SymbolValueLower = '%', SymbolValueUpper = '%' });
            //lines2.Add(new KeyboardKey() { CharValueLower = 'х', CharValueUpper = 'Х', SymbolValueLower = '&', SymbolValueUpper = '&' });
            //lines2.Add(new KeyboardKey() { CharValueLower = 'ј', CharValueUpper = 'Ј', SymbolValueLower = '/', SymbolValueUpper = '/' });
            //lines2.Add(new KeyboardKey() { CharValueLower = 'к', CharValueUpper = 'К', SymbolValueLower = '(', SymbolValueUpper = '(' });
            //lines2.Add(new KeyboardKey() { CharValueLower = 'л', CharValueUpper = 'Л', SymbolValueLower = ')', SymbolValueUpper = ')' });
            //lines2.Add(new KeyboardKey() { CharValueLower = 'ч', CharValueUpper = 'Ч', SymbolValueLower = '-', SymbolValueUpper = '_' });
            //lines2.Add(new KeyboardKey() { CharValueLower = 'ћ', CharValueUpper = 'Ћ', SymbolValueLower = '@', SymbolValueUpper = '@' });

            //lines2.Add(new KeyboardKey() { CharValueLower = 'ѕ', CharValueUpper = 'Ѕ', SymbolValueLower = '~', SymbolValueUpper = '~' });
            //lines2.Add(new KeyboardKey() { CharValueLower = 'џ', CharValueUpper = 'Џ', SymbolValueLower = '€', SymbolValueUpper = '€' });
            //lines2.Add(new KeyboardKey() { CharValueLower = 'ц', CharValueUpper = 'Ц', SymbolValueLower = '£', SymbolValueUpper = '£' });
            //lines2.Add(new KeyboardKey() { CharValueLower = 'в', CharValueUpper = 'В', SymbolValueLower = '¥', SymbolValueUpper = '¥' });
            //lines2.Add(new KeyboardKey() { CharValueLower = 'б', CharValueUpper = 'Б', SymbolValueLower = '\\', SymbolValueUpper = '\\' });
            //lines2.Add(new KeyboardKey() { CharValueLower = 'н', CharValueUpper = 'Н', SymbolValueLower = '[', SymbolValueUpper = '[' });
            //lines2.Add(new KeyboardKey() { CharValueLower = 'м', CharValueUpper = 'М', SymbolValueLower = ']', SymbolValueUpper = ']' });
            //lines2.Add(new KeyboardKey() { CharValueLower = ',', CharValueUpper = ';', SymbolValueLower = '<', SymbolValueUpper = '<' });
            //lines2.Add(new KeyboardKey() { CharValueLower = '.', CharValueUpper = ':', SymbolValueLower = '>', SymbolValueUpper = '>' });
            //lines2.Add(new KeyboardKey() { CharValueLower = 'ж', CharValueUpper = 'Ж', SymbolValueLower = '|', SymbolValueUpper = '|' });


            lines2.Add(new KeyboardKey() { CharValueLower = 'q', CharValueUpper = 'Q', SymbolValueLower = '1', SymbolValueUpper = '1' });
            lines2.Add(new KeyboardKey() { CharValueLower = 'w', CharValueUpper = 'W', SymbolValueLower = '2', SymbolValueUpper = '2' });
            lines2.Add(new KeyboardKey() { CharValueLower = 'e', CharValueUpper = 'E', SymbolValueLower = '3', SymbolValueUpper = '3' });
            lines2.Add(new KeyboardKey() { CharValueLower = 'r', CharValueUpper = 'R', SymbolValueLower = '4', SymbolValueUpper = '4' });
            lines2.Add(new KeyboardKey() { CharValueLower = 't', CharValueUpper = 'T', SymbolValueLower = '5', SymbolValueUpper = '5' });
            lines2.Add(new KeyboardKey() { CharValueLower = 'y', CharValueUpper = 'Y', SymbolValueLower = '6', SymbolValueUpper = '6' });
            lines2.Add(new KeyboardKey() { CharValueLower = 'u', CharValueUpper = 'U', SymbolValueLower = '7', SymbolValueUpper = '7' });
            lines2.Add(new KeyboardKey() { CharValueLower = 'i', CharValueUpper = 'I', SymbolValueLower = '8', SymbolValueUpper = '8' });
            lines2.Add(new KeyboardKey() { CharValueLower = 'o', CharValueUpper = 'O', SymbolValueLower = '9', SymbolValueUpper = '9' });
            lines2.Add(new KeyboardKey() { CharValueLower = 'p', CharValueUpper = 'P', SymbolValueLower = '0', SymbolValueUpper = '0' });
            lines2.Add(new KeyboardKey() { CharValueLower = '-', CharValueUpper = '_', SymbolValueLower = '{', SymbolValueUpper = '[' });
            lines2.Add(new KeyboardKey() { CharValueLower = '=', CharValueUpper = '+', SymbolValueLower = '}', SymbolValueUpper = ']' });

            lines2.Add(new KeyboardKey() { CharValueLower = 'a', CharValueUpper = 'A', SymbolValueLower = '!', SymbolValueUpper = '!' });
            lines2.Add(new KeyboardKey() { CharValueLower = 's', CharValueUpper = 'S', SymbolValueLower = '@', SymbolValueUpper = '@' });
            lines2.Add(new KeyboardKey() { CharValueLower = 'd', CharValueUpper = 'D', SymbolValueLower = '#', SymbolValueUpper = '#' });
            lines2.Add(new KeyboardKey() { CharValueLower = 'f', CharValueUpper = 'F', SymbolValueLower = '$', SymbolValueUpper = '$' });
            lines2.Add(new KeyboardKey() { CharValueLower = 'g', CharValueUpper = 'G', SymbolValueLower = '%', SymbolValueUpper = '%' });
            lines2.Add(new KeyboardKey() { CharValueLower = 'h', CharValueUpper = 'H', SymbolValueLower = '^', SymbolValueUpper = '^' });
            lines2.Add(new KeyboardKey() { CharValueLower = 'j', CharValueUpper = 'J', SymbolValueLower = '&', SymbolValueUpper = '&' });
            lines2.Add(new KeyboardKey() { CharValueLower = 'k', CharValueUpper = 'K', SymbolValueLower = '*', SymbolValueUpper = '*' });
            lines2.Add(new KeyboardKey() { CharValueLower = 'l', CharValueUpper = 'L', SymbolValueLower = '(', SymbolValueUpper = '(' });
            lines2.Add(new KeyboardKey() { CharValueLower = ';', CharValueUpper = ':', SymbolValueLower = ')', SymbolValueUpper = ')' });
            lines2.Add(new KeyboardKey() { CharValueLower = '\'', CharValueUpper = '"', SymbolValueLower = '\'', SymbolValueUpper = '"' });

            lines2.Add(new KeyboardKey() { CharValueLower = 'z', CharValueUpper = 'Z', SymbolValueLower = '~', SymbolValueUpper = '~' });
            lines2.Add(new KeyboardKey() { CharValueLower = 'x', CharValueUpper = 'X', SymbolValueLower = '€', SymbolValueUpper = '€' });
            lines2.Add(new KeyboardKey() { CharValueLower = 'c', CharValueUpper = 'C', SymbolValueLower = '£', SymbolValueUpper = '£' });
            lines2.Add(new KeyboardKey() { CharValueLower = 'v', CharValueUpper = 'V', SymbolValueLower = '¥', SymbolValueUpper = '¥' });
            lines2.Add(new KeyboardKey() { CharValueLower = 'b', CharValueUpper = 'B', SymbolValueLower = '\\', SymbolValueUpper = '\\' });
            lines2.Add(new KeyboardKey() { CharValueLower = 'n', CharValueUpper = 'N', SymbolValueLower = '[', SymbolValueUpper = '[' });
            lines2.Add(new KeyboardKey() { CharValueLower = 'm', CharValueUpper = 'M', SymbolValueLower = ']', SymbolValueUpper = ']' });
            lines2.Add(new KeyboardKey() { CharValueLower = ',', CharValueUpper = '<', SymbolValueLower = ',', SymbolValueUpper = '<' });
            lines2.Add(new KeyboardKey() { CharValueLower = '.', CharValueUpper = '>', SymbolValueLower = '.', SymbolValueUpper = '>' });
            lines2.Add(new KeyboardKey() { CharValueLower = '/', CharValueUpper = '?', SymbolValueLower = '|', SymbolValueUpper = '\\' });
        }

    }

    public class KeyboardKey
    {
        public char CharValueLower { get; set; }
        public char CharValueUpper { get; set; }

        public char SymbolValueLower { get; set; }
        public char SymbolValueUpper { get; set; }
    }
}