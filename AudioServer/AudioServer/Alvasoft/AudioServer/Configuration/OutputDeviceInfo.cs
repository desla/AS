using System;
using Alvasoft.Utils.Common;

namespace Alvasoft.AudioServer.Configuration
{
    /// <summary>
    ///     Описание устройства вывода.
    ///     <para>
    ///         Идентификатор (<see cref="Alvasoft.Utils.Common.Identifiable.GetId" />),
    ///         является уникальным, адресуемым свойством для устройства вывода.
    ///     </para>
    ///     <para>
    ///         Имя (<see cref="Alvasoft.Utils.Common.Nameable.GetName" />),
    ///         является уникальным, адресуемым свойством для устройства вывода.
    ///     </para>
    /// </summary>
    public interface OutputDeviceInfo : IdentifiableNameable
    {
        /// <summary>
        ///     Возвращает количество описаний каналов.
        /// </summary>
        /// <returns>Количество описаний каналов.</returns>
        int GetChannelsCount();

        /// <summary>
        ///     Возвращает описание канала по индексу.
        /// </summary>
        /// <param name="aIndex">Индекс.</param>
        /// <returns>Описание канала.</returns>
        /// <exception cref="IndexOutOfRangeException">
        ///     Неверный индекс. Допустимый диаппазон значений [0 ..
        ///     <see cref="GetChannelsCount()" /> - 1]>
        /// </exception>
        OutputChannelInfo GetChannel(int aIndex);

        /// <summary>
        ///     Возвращает тип устройства.
        /// </summary>
        /// <returns>Тип устройства.</returns>
        DeviceType GetDeviceType();

        /// <summary>
        ///     Устанавливает тип устройтсва. По-умолчания - default.
        /// </summary>
        /// <param name="aStrDeviceType">Тип устройства в строковом формате.</param>
        /// <exception cref="NotImplementedException">Возникает, если тип не найден.</exception>
        void SetDeviceType(string aStrDeviceType);
    }
}