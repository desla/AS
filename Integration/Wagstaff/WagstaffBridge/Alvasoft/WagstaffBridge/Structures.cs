using System;

namespace Alvasoft.WagstaffBridge
{
    /// <summary>
    /// Данные из OPC.
    /// </summary>
    public class DataValue
    {
        /// <summary>
        /// Для сохранения в БУФЕРНУЮ ТАБЛИЦУ и удаления из нее.
        /// </summary>
        public int Id { get; set; } 
        /// <summary>
        /// Идентификатор типа.
        /// </summary>
        public int TypeId { get; set; } 
        /// <summary>
        /// Идентификатор объекта.
        /// </summary>
        public int ObjectId { get; set; } 
        /// <summary>
        /// Идентификатор данных.
        /// </summary>
        public int DataId { get; set; } 
        /// <summary>
        /// Време чтения данных.
        /// </summary>
        public DateTime ValueTime { get; set; } 
        /// <summary>
        /// Значение.
        /// </summary>
        public double Value { get; set; } 
        /// <summary>
        /// Имя типа - для сохранения в БУФЕРНУЮ ТАБЛИЦУ.
        /// </summary>
        public string TypeName { get; set; } 
        /// <summary>
        /// Имя объекта.
        /// </summary>
        public string ObjectName { get; set; } 
        /// <summary>
        /// Имя значения.
        /// </summary>
        public string DataName { get; set; } 
    }

    /// <summary>
    /// Информация для конфигруации.
    /// </summary>
    public class DataReadInfo
    {
        /// <summary>
        /// Идентификатор типа.
        /// </summary>
        public int TypeId { get; set; }
        /// <summary>
        /// Идентификатор объекта.
        /// </summary>
        public int ObjectId { get; set; }
        /// <summary>
        /// Идентификатор данных.
        /// </summary>
        public int DataId { get; set; } 
        /// <summary>
        /// Имя OPC тега.
        /// </summary>
        public string OpcItemName { get; set; }
        /// <summary>
        /// Имя типа
        /// </summary>
        public string TypeName { get; set; }
        /// <summary>
        /// Имя объекта.
        /// </summary>
        public string ObjectName { get; set; }
        /// <summary>
        /// Имя значения.
        /// </summary>
        public string DataName { get; set; }
    }

    /// <summary>
    /// Рецепт.
    /// </summary>
    public class RecipeInfo
    {
        /// <summary>
        /// Идентификатор.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Имя рецепта.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Ревизия рецепта.
        /// </summary>
        public int Revision { get; set; }
        /// <summary>
        /// Технология.
        /// </summary>
        public string Technology { get; set; }
        /// <summary>
        /// Сплав.
        /// </summary>
        public string Alloy { get; set; }
        /// <summary>
        /// Время создания.
        /// </summary>
        public DateTime CreationTime { get; set; }
        /// <summary>
        /// Метка архивности.
        /// </summary>
        public bool IsArchived { get; set; }
    }

    /// <summary>
    /// Плавка.
    /// </summary>
    public class CastSchedule
    {
        /// <summary>
        /// Идентификатор.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Имя рецепта.
        /// </summary>
        public string RecipeName { get; set; }
        /// <summary>
        /// Ревизия.
        /// </summary>
        public int RecipeRevision { get; set; }
        /// <summary>
        /// Номер плавки.
        /// </summary>
        public string CastNumber { get; set; }
        /// <summary>
        /// Состояние.
        /// </summary>
        public int ScheduleState { get; set; }
        /// <summary>
        /// Приоритет.
        /// </summary>
        public int Priority { get; set; }
    }

    /// <summary>
    /// Состояние плавки в таблице CAST_SCHEDULE
    /// </summary>
    public enum ScheduleState
    {
        /// <summary>
        /// Не обработано.
        /// </summary>
        NOT_PROCESSED = 0,
        /// <summary>
        /// Передано.
        /// </summary>
        WAS_SAVED = 1,
        /// <summary>
        /// Ошибка передачи.
        /// </summary>
        WAS_ERROR = 2
    };
}
