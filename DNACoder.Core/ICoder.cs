namespace DNACoder.Core
{
    /// <summary>
    /// Интерфейс, описывающий кодер для работы с потоком данных
    /// </summary>
    public interface ICoder
    {
        /// <summary>
        /// Длина одного кодового слова
        /// </summary>
        int WordLength { get; }

        /// <summary>
        /// Преобразует байт в кодовое слово с использованием словаря
        /// </summary>
        /// <param name="value">Байт для преобразвоания в кодовое слово</param>
        /// <returns></returns>
        string Encode(byte value);

        /// <summary>
        /// Преобразует кодовое слово в байт
        /// </summary>
        /// <param name="codeWord">Кодовое слово для преобразования</param>
        /// <returns></returns>
        byte Decode(string codeWord);
    }
}