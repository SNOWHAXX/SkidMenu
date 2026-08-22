namespace SkidMenu.ui
{
    public class Notification
    {
        public readonly string title;
        public string message;
        public readonly float ttl;
        public float lifetime;
        public float slideProgress;
        public bool dying;
        public float deathProgress;

        public Notification(string title, string message, float ttl)
        {
            this.title = title;
            this.message = message;
            this.ttl = ttl;
            this.lifetime = 0;
            this.slideProgress = 0;
            this.dying = false;
            this.deathProgress = 0;
        }

        public bool HasExpired
        {
            get { return this.lifetime > ttl; }
        }
    }
}
