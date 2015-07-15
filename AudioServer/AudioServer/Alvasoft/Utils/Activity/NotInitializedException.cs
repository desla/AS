using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alvasoft.Utils.Activity {
   /// <summary>
   /// Не инициализован.
   /// </summary>
   public class NotInitializedException : Exception {
      /// <summary>
      /// Конструктор по умолчанию.
      /// </summary>
      public NotInitializedException()
         : base( "Не инициализован." ) {
      }
   }
}
