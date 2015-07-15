namespace Alvasoft.Utils.Activity {
   /// <summary>
   /// Инициализируемое (инициализация/деинициализация).
   /// <para>Базовый интерфейс.</para>
   /// </summary>
   public interface Initializable {
      /// <summary>
      /// Состоянии инициализации.
      /// </summary>
      /// <returns><code>true</code>, если инициализован.</returns>
      bool IsInitialized();

      /// <summary>
      /// Инициализация.
      /// </summary>
      /// <exception cref="InitializedException">Если уже инициализован.</exception>
      void Initialize();

      /// <summary>
      /// Деинициализация.
      /// </summary>
      /// <exception cref="NotInitializedException">Если не инициализован.</exception>
      void Uninitialize();
   }
}
