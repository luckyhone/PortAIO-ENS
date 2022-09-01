using ExorAIO.Core;

#pragma warning disable 1587

namespace ExorAIO
{

    internal class Program
    {
        #region Methods

        /// <summary>
        ///     The entry point of the application.
        /// </summary>
        public static void Loads()
        {
            /// <summary>
            ///     Loads the Bootstrap.
            /// </summary>
            //ootstrap.Init();
            /// <summary>
            ///     Loads the AIO.
            /// </summary>
            Aio.OnLoad();
        }

        #endregion
    }
}