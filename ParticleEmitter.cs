using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using Microsoft.VisualBasic.CompilerServices;
//using System.Numerics;

namespace Lunar_Lander
{
    public class Particle
    {
        public int name;
        public Vector2 position;
        public float rotation;
        public Vector2 direction;
        public float speed;
        public TimeSpan lifetime;
        public Texture2D texture;

        public Particle(int name, Vector2 position, Vector2 direction, float speed, TimeSpan lifetime, Texture2D texture)
        {
            this.name = name;
            this.position = position;
            this.direction = direction;
            this.speed = speed;
            this.lifetime = lifetime;
            this.texture = texture;

            this.rotation = 0;
        }
    }

    class ParticleEmitter 
    {
        private Dictionary<int, Particle> m_particles = new Dictionary<int, Particle>();
        private Texture2D m_texSmoke;
        private Texture2D m_texFire;
        private MyRandom random = new MyRandom();

        private TimeSpan m_rate;
        private int m_sourceX;
        private int m_sourceY;
        private int m_particleSize;
        private int m_speed;
        private TimeSpan m_lifetime;
        private TimeSpan m_switchover;
        private float landerAngle;
        public Vector2 Gravity { get; set; }

        public ParticleEmitter(ContentManager content, TimeSpan rate, int sourceX, int sourceY, int size, int speed, TimeSpan lifetime, TimeSpan switchover)
        {
            m_rate = rate;
            m_sourceX = sourceX;
            m_sourceY = sourceY;
            m_particleSize = size;
            m_speed = speed;
            m_lifetime = lifetime;
            m_switchover = switchover;

            m_texSmoke = content.Load<Texture2D>("smoke");
            m_texFire = content.Load<Texture2D>("fire");

            this.Gravity = new Vector2(0, 0);
        }

        public int ParticleCount
        {
            get { return m_particles.Count; }
        }

        private TimeSpan m_accumulated = TimeSpan.Zero;

        /// <summary>
        /// Generates new particles, updates the state of existing ones and retires expired particles.
        /// </summary>
        /// 
        public void shipCrash(Vector2 position)
        {
            m_rate = new TimeSpan(0, 0, 0, 0, 5);
            m_sourceX = (int)position.X;
            m_sourceY = (int)position.Y;
            m_particleSize = 60;
            m_speed = 150;
            m_lifetime = new TimeSpan(0, 0, 0, 0, 500);
            m_switchover = new TimeSpan(0, 0, 0, 0, 100);

        }
        public void shipThrust(GameTime gameTime, Vector2 position, bool emitParticles, float angle, double dev = 0)
        {
            if (emitParticles)
            {
                m_sourceX = (int)position.X;
                m_sourceY = (int)position.Y;
                landerAngle = angle;
                // Generate particles at the specified rate
                m_accumulated += gameTime.ElapsedGameTime;
                while (m_accumulated > m_rate)
                {
                    m_accumulated -= m_rate;

                    //for (int i = 0; i < 15; i++)
                    {
                        Particle p = new Particle(
                            random.Next(),
                            new Vector2(m_sourceX, m_sourceY),
                            random.nextCircleVector(landerAngle, dev),
                            (float)random.nextGaussian(m_speed, Math.Sqrt(m_speed)),
                            m_lifetime,
                            m_texFire);

                        if (!m_particles.ContainsKey(p.name))
                        {
                            m_particles.Add(p.name, p);
                        }
                    }
                }
            }
            //
            // For any existing particles, update them, if we find ones that have expired, add them
            // to the remove list.
            List<int> removeMe = new List<int>();
            foreach (Particle p in m_particles.Values)
            {
                p.lifetime -= gameTime.ElapsedGameTime;
                if (p.lifetime < TimeSpan.Zero)
                {
                    //
                    // Add to the remove list
                    removeMe.Add(p.name);
                }
                else
                {
                    //
                    // Only if we have enough elapsed time, and then move/rotate things
                    // based upon elapsed time, not just the fact that we have received an update.
                    if (gameTime.ElapsedGameTime.Milliseconds > 0)
                    {
                        //
                        // Update its position
                        p.position += (p.direction * (p.speed * (gameTime.ElapsedGameTime.Milliseconds / 1000.0f)));

                        //
                        // Have it rotate proportional to its speed
                        p.rotation += (p.speed * (gameTime.ElapsedGameTime.Milliseconds / 100000.0f));
                    }

                    if (p.texture != m_texSmoke && p.lifetime < m_switchover)
                    {
                        p.texture = m_texSmoke;
                    }

                    //
                    // Apply some gravity
                    p.direction += this.Gravity;
                }
            }

            //
            // Remove any expired particles
            foreach (int Key in removeMe)
            {
                m_particles.Remove(Key);
            }
        }

        /// <summary>
        /// Renders the active particles
        /// </summary>
        public void Draw(SpriteBatch spriteBatch)
        {

            Rectangle r = new Rectangle(0, 0, m_particleSize, m_particleSize);
            foreach (Particle p in m_particles.Values)
            {
                r.X = (int)p.position.X;
                r.Y = (int)p.position.Y;
                spriteBatch.Draw(
                    p.texture,
                    r,
                    null,
                    Color.White,
                    p.rotation,
                    new Vector2(p.texture.Width / 2, p.texture.Height / 2),
                    SpriteEffects.None,
                    0);
            }
        }
    }

    /// <summary>
    /// Expands upon some of the features the .NET Random class does:
    /// 
    /// *NextRange : Generate a random number within some range
    /// *NextGaussian : Generate a normally distributed random number
    /// 
    /// </summary>
    class MyRandom : Random
    {

        /// <summary>
        /// Generates a random number in the range or [Min,Max]
        /// </summary>
        public float nextRange(float min, float max)
        {
            return MathHelper.Lerp(min, max, (float)this.NextDouble());
        }

        /// <summary>
        /// Generate a random vector about a unit circle
        /// </summary>
        public Vector2 nextCircleVector(float angle = -1, double stdDev = 0.1)
        {
            if(angle == -1)
                angle = (float)(this.NextDouble() * 2.0 * Math.PI);
            else
            {
                angle = (float)nextGaussian(angle, stdDev);
            }
            float x = (float)Math.Cos(angle + Math.PI/2);
            float y = (float)Math.Sin(angle + Math.PI / 2);

            return new Vector2(x, y);
        }

        /// <summary>
        /// Generate a normally distributed random number.  Derived from a Wiki reference on
        /// how to do this.
        /// </summary>
        public double nextGaussian(double mean, double stdDev)
        {
            if (this.usePrevious)
            {
                this.usePrevious = false;
                return mean + y2 * stdDev;
            }
            this.usePrevious = true;

            double x1 = 0.0;
            double x2 = 0.0;
            double y1 = 0.0;
            double z = 0.0;

            do
            {
                x1 = 2.0 * this.NextDouble() - 1.0;
                x2 = 2.0 * this.NextDouble() - 1.0;
                z = (x1 * x1) + (x2 * x2);
            }
            while (z >= 1.0);

            z = Math.Sqrt((-2.0 * Math.Log(z)) / z);
            y1 = x1 * z;
            y2 = x2 * z;

            return mean + y1 * stdDev;
        }

        /// <summary>
        /// Keep this around to optimize gaussian calculation performance.
        /// </summary>
        private double y2;
        private bool usePrevious { get; set; }
    }
}



