namespace Alvasoft.WagstaffBridge.OpcDataReader
{
    /// <summary>
    /// Обратная связь при получении данных от DataReader.
    /// </summary>
    public interface DataReaderCallback
    {
        /// <summary>
        /// Данные получены.
        /// </summary>
        /// <param name="aDataValue">Данные.</param>
        void OnReadedData(DataValue aDataValue);
    }
}
