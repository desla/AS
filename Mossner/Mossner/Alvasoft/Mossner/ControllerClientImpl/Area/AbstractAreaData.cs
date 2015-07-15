namespace Alvasoft.Mossner.ControllerClientImpl.Area
{
    /// <summary>
    /// Класс для наследования. Содержит Исходные DataItem'ы.
    /// </summary>
    public class AbstractAreaData
    {        
        public DataItem IdCheckOutputItem { get; protected set; }
        public DataItem IdNumberOutputItem { get; protected set; }
        public DataItem IdCheckInputItem { get; protected set; }
        public DataItem IdNumberInputItem { get; protected set; }               
    }

    /// <summary>
    /// Класс для наследования.
    /// </summary>
    public class AbstractAreaData2 : AbstractAreaData
    {
        public DataItem WeightOutputItem { get; protected set; }
        public DataItem LengthOutputItem { get; protected set; }
    }
}
