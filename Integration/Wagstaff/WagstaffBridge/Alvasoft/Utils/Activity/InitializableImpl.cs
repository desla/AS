using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alvasoft.Utils.Activity {
   /// <summary>
   /// Реализация инициализируемого (инициализация/деинициализация).
   /// </summary>
   /// <remarks>
   /// Базовый класс для реализации инициализируемого.
   /// </remarks>
   public class InitializableImpl : Initializable {
      private bool initialized;

      /// <summary>
      /// Конструктор по умолчанию.
      /// </summary>
      public InitializableImpl() {
         // Не инициализован.
         initialized = false;
      }

      /// <inheritdoc />
      public bool IsInitialized() {
         return initialized;
      }

      /// <inheritdoc />
      public void Initialize() {
         CheckNotInitialized();
         // Инициализован.
         initialized = true;
         // Выполняем инициализацию.
         DoInitialize();
      }

      /// <inheritdoc />
      public void Uninitialize() {
         CheckInitialized();
         // Не инициализован.
         initialized = false;
         // Выполняем деинициализацию.
         DoUninitialize();
      }

      /// <summary>
      /// Выполняет инициализацию.
      /// <para>
      /// По умолчанию ничего не делает. При необходимости перекрывается в потомке.
      /// </summary>
      protected virtual void DoInitialize() { }

      /// <summary>
      /// Выполняет деинициализацию.
      /// <para>
      /// Внимание: во время вызова этого метода не инициализован!
      /// По умолчанию ничего не делает. При необходимости перекрывается в потомке.
      /// </summary>
      protected virtual void DoUninitialize() { }

      /// <summary>
      /// Выполняет проверку инициализации.
      /// </summary>
      /// <exception cref="NotInitializedException">Если не инициализован.</exception>      
      protected void CheckInitialized() {
         if( !initialized ) {
            // Не инициализован.
            throw new NotInitializedException();
         }
      }

      /// <summary>
      /// Выполняет проверку не инициализации.
      /// </summary>
      /// <exception cref="InitializedException">Если инициализован.</exception>
      protected void CheckNotInitialized() {
         if( initialized ) {
            // Инициализован.
            throw new InitializedException();
         }
      }
   }
}
