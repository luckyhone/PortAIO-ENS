using System.Collections.Generic;
using SharpDX;

namespace Entropy.Lib.Render
{
    public class Triangle : LinearPolygon
    {
        #region Methods

        protected override void Update()
        {
            WorldPoints = new List<Vector3>
            {
                RootPoint1, RootPoint2, RootPoint3
            };
        }

        #endregion

        #region Fields

        protected Vector3 _rootPoint1;

        protected Vector3 _rootPoint2;

        protected Vector3 _rootPoint3;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Triangle" /> class.
        /// </summary>
        /// <param name="worldRoot1">The first world-space root point.</param>
        /// <param name="worldRoot2">The second world-space root point.</param>
        /// <param name="worldRoot3">The third world-space root point.</param>
        public Triangle(Vector3 worldRoot1, Vector3 worldRoot2, Vector3 worldRoot3)
        {
            _rootPoint1 = worldRoot1;
            _rootPoint2 = worldRoot2;
            _rootPoint3 = worldRoot3;

            Update();
        }

        protected Triangle()
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the root point.
        /// </summary>
        /// <value>
        ///     The first root point.
        /// </value>
        public Vector3 RootPoint1
        {
            get => _rootPoint1;

            set
            {
                _rootPoint1 = value;
                Update();
            }
        }

        /// <summary>
        ///     Gets or sets the second root point.
        /// </summary>
        /// <value>
        ///     The second root point.
        /// </value>
        public Vector3 RootPoint2
        {
            get => _rootPoint2;

            set
            {
                _rootPoint2 = value;
                Update();
            }
        }

        /// <summary>
        ///     Gets or sets the third root point.
        /// </summary>
        /// <value>
        ///     The third root point.
        /// </value>
        public Vector3 RootPoint3
        {
            get => _rootPoint3;

            set
            {
                _rootPoint3 = value;
                Update();
            }
        }

        #endregion
    }
}