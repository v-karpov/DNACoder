using DNACoder.Core;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace DNACoder.Wpf
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Путь к открытому файлу
        /// </summary>
        public string FilePath { get; set; } = "(файл не открыт)";

        /// <summary>
        /// Поток, открытый в программе в текущий момент
        /// </summary>
        public Stream Stream { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    Stream?.Close();

                    if (dialog.FileName.EndsWith("fasta"))
                    {
                        Stream = EncodedStream.Open(dialog.FileName, FileMode.Open);
                    }
                    else
                    {
                        Stream = File.OpenRead(dialog.FileName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось открыть данный файл: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                filePath.Text = FilePath = dialog.FileName;
                fileSize.Content = "Размер: " + (Stream.Length > 1024
                    ? Stream.Length / 1024 + " кб."
                    : Stream.Length + " б.");

                var wordLength = (Stream as EncodedStream)?.Coder.WordLength ?? 0;

                formatType.Content = "Формат: " + (wordLength == 4 
                    ? "4 CC" 
                    : wordLength > 0
                        ? "3 CC" 
                        : "не законидированный");

                codeWordLength.Content = "Длина кодового слова: " + wordLength;

                noCoder.IsChecked = wordLength == 0;
                coder3Button.IsChecked = wordLength == 3;
                coder4Button.IsChecked = wordLength != 3;

                saveButton.IsEnabled = true;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();

            var wordLength = coder3Button.IsChecked == true
                   ? 3
                   : coder4Button.IsChecked == true
                       ? 4
                       : 0;

            if (dialog.ShowDialog() == true)
            {
                var destinationStream = default(Stream);

                switch (wordLength)
                {
                    case 3:
                        destinationStream = new Encoded3Stream(dialog.FileName, FileMode.OpenOrCreate);
                        break;
                    case 4:
                        destinationStream = new Encoded4Stream(dialog.FileName, FileMode.OpenOrCreate);
                        break;
                    default:
                        destinationStream = File.OpenWrite(dialog.FileName);
                        break;
                }

                Stream.CopyTo(destinationStream);
                destinationStream.Close();

                MessageBox.Show("Файл успешно сохранен", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
