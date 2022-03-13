using System;

namespace Sylves
{
    public struct Connection
    {
        public bool Mirror { get; set; }
        public int Rotation { get; set; }
        public int Sides { get; set; }

        public static Connection operator*(Connection a, Connection b)
        {
            if (a.Rotation != 0 || b.Rotation != 0)
                throw new NotImplementedException();

            return new Connection
            {
                Mirror = a.Mirror ^ b.Mirror,
            };
        }

        public Connection GetInverse()
        {
            if (Rotation != 0)
                throw new NotImplementedException();
            return new Connection
            {
                Mirror = Mirror,
            };
        }
    }
}
