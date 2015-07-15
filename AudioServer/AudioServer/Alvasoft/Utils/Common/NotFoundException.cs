using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alvasoft.Utils.Common {
   /// <summary>
   /// Не найден.
   /// </summary>
   public class NotFoundException : Exception {
      /// <summary>
      /// Конструктор по умолчанию.
      /// </summary>
      public NotFoundException()
         : base( "Не найден." ) {
      }
   }
}
