using UnityEngine;

namespace AsteroidGame
{
    public static class GameConstants
    {
        public static int MAX_PLAYER_HEALTH = 5;
        public static float PLAYER_FIRE_IMPULSE = 100f;

        #region AI

        public static string ASTEROID_POOL_PARENT_NAME = "Asteroids";
        public static int MIN_ASTEROIDS = 5;
        public static int MAX_ASTEROIDS = 10;

        public static float ASTEROID_FREE_ZONE = 3f;

        public static string AI_PATH_PARENT_NAME = "AI paths";
        public static string AI_MANAGER_NAME = "AI Manager";
        public static float AI_FIRE_MIN_INTERVAL = 5.0f;
        public static float AI_FIRE_MAX_INTERVAL = 10.0f;
        public static float AI_SPAWN_COOLDOWN = 5.0f;

        public static float AI_MIN_INACCURACY_IN_DEGREES = 0f;
        public static float AI_MAX_INACCURACY_IN_DEGREES = 10f;

        #endregion // AI

        #region Combat

        public static string BULLET_POOL_PARENT_NAME = "Bullets";
        public static float BULLET_LIFE_IN_SECONDS = 0.8f;
        public static float BULLET_SPEED = 15f;

        public static string MISSSILE_POOL_PARENT_NAME = "Missiles";
        public static float MISSILE_LIFE_IN_SECONDS = 3f;
        public static float MISSILE_SPEED = 3f;

        public static int BONUS_WEAPON_TIME_IN_SECONDS = 5;
        public static int WEAPON_POOL_SIZE = 5;

        // Make sure this is in sync with layers in project settings
        public static int NO_COLLISION_LAYER = 0;
        public static int PLAYER_AMMO_LAYER = 9;
        public static int ALIEN_AMMO_LAYER = 10;

        public static Color DEFAULT_AMMO_COLOR = Color.white;
        public static Color PLAYER_AMMO_COLOR = Color.green;
        public static Color ALIEN_AMMO_COLOR = Color.red;

        #endregion // Combat
    }
}