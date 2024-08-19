using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlTypes;

namespace HereHaveAPaSSword
{

    public partial class MainWindow : Window
    {
        private GeneratePasswordsService _generatePasswordsService;
        public MainWindow()
        {
            _generatePasswordsService = new GeneratePasswordsService();
            InitializeComponent();
        }

        bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c == ' ')
                    continue;

                if (c < '0' || c > '9')
                    return false;
            }
            return true;
        }

        private void GeneratePasswords(object sender, RoutedEventArgs e)
        {
            RefreshTextBoxes();
            PasswordList.ItemsSource = null;

            if (string.IsNullOrWhiteSpace(TotalCharacters.Text)) {
                ErrorsLabel.Background = Brushes.White;
                TotalCharacters.Background = Brushes.LightPink;
                ErrorsTextBlock.Text = "Numarul total de caractere este obligatoriu!!!!";
                return;
            }

            if (!IsDigitsOnly(AlphabetMin.Text) || !IsDigitsOnly(AlphabetMax.Text) ||
                !IsDigitsOnly(NumericMin.Text) || !IsDigitsOnly(NumericMax.Text) || 
                !IsDigitsOnly(SpecialMin.Text) || !IsDigitsOnly(SpecialMax.Text) || !IsDigitsOnly(TotalCharacters.Text))
            {
                if (!IsDigitsOnly(AlphabetMin.Text)) { AlphabetMin.Background = Brushes.LightPink; };
                if (!IsDigitsOnly(AlphabetMax.Text)) { AlphabetMax.Background = Brushes.LightPink; };
                if (!IsDigitsOnly(NumericMin.Text)) { NumericMin.Background = Brushes.LightPink; };
                if (!IsDigitsOnly(NumericMax.Text)) { NumericMax.Background = Brushes.LightPink; };
                if (!IsDigitsOnly(SpecialMin.Text)) { SpecialMin.Background = Brushes.LightPink; };
                if (!IsDigitsOnly(SpecialMax.Text)) { SpecialMax.Background = Brushes.LightPink; };
                if (!IsDigitsOnly(TotalCharacters.Text)) { TotalCharacters.Background = Brushes.LightPink; };

                ErrorsLabel.Background = Brushes.White;
                ErrorsTextBlock.Text = "Se pot introduce doar cifre sau spatii goale!!!!";
                return;
            }

            var alphabetMin = string.IsNullOrWhiteSpace(AlphabetMin.Text) ? -1 : Int32.Parse(AlphabetMin.Text.Replace(" ", ""));
            var alphabetMax = string.IsNullOrWhiteSpace(AlphabetMax.Text) ? -1 : Int32.Parse(AlphabetMax.Text.Replace(" ", ""));
            var numericMin = string.IsNullOrWhiteSpace(NumericMin.Text) ? -1 : Int32.Parse(NumericMin.Text.Replace(" ", ""));
            var numericMax = string.IsNullOrWhiteSpace(NumericMax.Text) ? -1 : Int32.Parse(NumericMax.Text.Replace(" ", ""));
            var specialMin = string.IsNullOrWhiteSpace(SpecialMin.Text) ? -1 : Int32.Parse(SpecialMin.Text.Replace(" ", ""));
            var specialMax = string.IsNullOrWhiteSpace(SpecialMax.Text) ? -1 : Int32.Parse(SpecialMax.Text.Replace(" ", ""));
            var totalCharacters = Int32.Parse(TotalCharacters.Text.Replace(" ", ""));

            if (!ValidateNumbers(totalCharacters, alphabetMin, alphabetMax, numericMin, numericMax, specialMin, specialMax))
            {
                return;
            }
            
            PasswordList.ItemsSource = _generatePasswordsService.GetPasswords(totalCharacters, alphabetMin,alphabetMax, numericMin, numericMax, specialMin, specialMax);

        }

        private bool ValidateNumbers(int totalCharacters, int alphabetMin, int alphabetMax, int numericMin, int numericMax, int specialMin, int specialMax)
        {
            if (totalCharacters > 30 || totalCharacters < 6)
            {
                TotalCharacters.Background = Brushes.LightPink;
                ErrorsLabel.Background = Brushes.White;
                ErrorsTextBlock.Text = "Sunt permise parole de minim 6 si maxim 30 de caractere!!";
                return false;
            }

            if (specialMax != -1 && alphabetMax != -1 && numericMax != -1 && specialMax + alphabetMax + numericMax < totalCharacters)
            {
                AlphabetMax.Background = Brushes.LightPink;
                NumericMax.Background = Brushes.LightPink;
                SpecialMax.Background = Brushes.LightPink;
                ErrorsLabel.Background = Brushes.White;
                ErrorsTextBlock.Text = "Numarul de caractere maxim este mai mic decat numarul total de caractere dorite!!!!";
                return false;
            }

            bool caractereMinime = true;
            if (specialMin != -1 && alphabetMin != -1 && numericMin != -1 && specialMin + alphabetMin + numericMin > totalCharacters)
            {
                AlphabetMin.Background = Brushes.LightPink;
                NumericMin.Background = Brushes.LightPink;
                SpecialMin.Background = Brushes.LightPink;
                caractereMinime =  false;
            }

            if (alphabetMin != -1 && alphabetMin > totalCharacters){AlphabetMin.Background = Brushes.LightPink; caractereMinime = false; }
            if (numericMin != -1 && numericMin > totalCharacters) { NumericMin.Background = Brushes.LightPink; caractereMinime = false; }
            if (specialMin != -1 && specialMin > totalCharacters) { SpecialMin.Background = Brushes.LightPink; caractereMinime = false; }

            if (alphabetMin != -1 && numericMin != -1 && alphabetMin + numericMin > totalCharacters) { 
                AlphabetMin.Background = Brushes.LightPink; NumericMin.Background = Brushes.LightPink; caractereMinime = false;
            }

            if (specialMin != -1 && numericMin != -1 && specialMin + numericMin > totalCharacters)
            {
                SpecialMin.Background = Brushes.LightPink; NumericMin.Background = Brushes.LightPink; caractereMinime = false;
            }

            if (alphabetMin != -1 && specialMin != -1 && alphabetMin + specialMin > totalCharacters)
            {
                AlphabetMin.Background = Brushes.LightPink; SpecialMin.Background = Brushes.LightPink; caractereMinime = false;
            }

            if (!caractereMinime)
            {
                ErrorsLabel.Background = Brushes.White;
                ErrorsTextBlock.Text = "Numarul de caractere minim este mai mare decat numarul total de caractere dorite!!!!";
                return false;
            }

            bool comparareNumere = true;
            if (alphabetMax != -1 && alphabetMin != -1 && alphabetMin > alphabetMax)
            {
                AlphabetMin.Background = Brushes.LightPink;
                AlphabetMax.Background = Brushes.LightPink;
                comparareNumere = false;
            }

            if (numericMax != -1 && numericMin != -1 && numericMin > numericMax)
            {
                NumericMin.Background = Brushes.LightPink;
                NumericMax.Background = Brushes.LightPink;
                comparareNumere = false;
            }

            if (specialMax != -1 && specialMin != -1 && specialMin > specialMax)
            {
                SpecialMin.Background = Brushes.LightPink;
                SpecialMax.Background = Brushes.LightPink;
                comparareNumere = false;
            }

            if (!comparareNumere)
            {
                ErrorsLabel.Background = Brushes.White;
                ErrorsTextBlock.Text = "Numarul minim de caractere de un fel nu poate fi mai mare decat maximul de caractere de acelasi fel";
                return false;
            }

            return true; ;
        }

        private void RefreshTextBoxes ()
        {
            AlphabetMin.Background = Brushes.White;
            AlphabetMax.Background = Brushes.White;
            NumericMin.Background = Brushes.White;
            NumericMax.Background = Brushes.White;
            SpecialMin.Background = Brushes.White;
            SpecialMax.Background = Brushes.White;
            TotalCharacters.Background = Brushes.White;
            ErrorsLabel.Background = Brushes.Transparent;
            ErrorsTextBlock.Text = "";
        }
    }
}
