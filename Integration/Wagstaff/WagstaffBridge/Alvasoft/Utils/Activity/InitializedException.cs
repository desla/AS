using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alvasoft.Utils.Activity {
   /// <summary>
   /// Инициализован.
   /// </summary>
   public class InitializedException : Exception {
      /// <summary>
      /// Конструктор по умолчанию.
      /// </summary>
      public InitializedException()
         : base( "Инициализован." ) {
      }
   }
}
