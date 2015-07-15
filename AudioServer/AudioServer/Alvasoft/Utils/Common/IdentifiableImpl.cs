using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alvasoft.Utils.Common {
   /// <summary>
   /// Реализация идентифицируемого.
   /// </summary>
   /// <para>Базовый класс для реализации идентифицируемого.</para>
   public class IdentifiableImpl : Identifiable {
      private long id;

      /// <summary>
      /// Конструктор по умолчанию.
      /// </summary>
      public IdentifiableImpl() {
         id = 0;
      }

      /// <inheritdoc />
      public long GetId() {
         return id;
      }

      /// <inheritdoc />
      public void SetId( long aId ) {
         id = aId;
      }
   }
}
