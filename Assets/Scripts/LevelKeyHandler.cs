using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class LevelKeyHandler
{
    public class LevelConfig
    {
        public int randomSeed;
        public float speed;
        public float maxIntensity;
        public int milestoneInterval;
        public bool includeTutorialChunks;
        public List<Player.Skill> includedSkills;
        public int version;

        public LevelConfig(
            int randomSeed = 0,
            float speed = 5f,
            float maxIntensity = 7.5f,
            int milestoneInterval = 10,
            bool includeTutorialChunks = true,
            List<Player.Skill> includedSkills = null,
            int version = 1)
        {
            this.randomSeed = randomSeed;
            this.speed = speed;
            this.maxIntensity = maxIntensity;
            this.milestoneInterval = milestoneInterval;
            this.includeTutorialChunks = includeTutorialChunks;
            this.includedSkills = includedSkills == null ?
                new List<Player.Skill> { Player.Skill.Dash, Player.Skill.Slide, Player.Skill.WallClimb, Player.Skill.WallJump } :
                includedSkills;
            this.version = version;
        }

        public override string ToString()
        {
            string str = $"Version: {version}, Seed: {randomSeed}, Speed: {speed}, Intensity: {maxIntensity}, Milestone Interval: {milestoneInterval}, Tutorial: {includeTutorialChunks}";
            int i = 1;
            foreach (Player.Skill skill in includedSkills)
            {
                str += $", Skill {i}: {skill}";
                i++;
            }
            return str;
        }
    }

    public static Dictionary<Player.Skill, int> skillDictionary = new Dictionary<Player.Skill, int> {
        {Player.Skill.Dash, 1},
        {Player.Skill.Slide, 2},
        {Player.Skill.WallClimb, 3},
        {Player.Skill.WallJump, 4}
    };

    public static string GenerateKey(LevelConfig levelConfig)
    {
        string key = "";
        // Seed
        key += levelConfig.randomSeed;

        // Speed
        key += "S";
        key += (int)(levelConfig.speed * 100); // to 2dp

        // Intensity
        key += "I";
        key += (int)(levelConfig.maxIntensity * 100); // to 2dp

        // Milestone interval
        key += "M";
        key += levelConfig.milestoneInterval;

        // Tutorial chunks
        key += "T";
        key += levelConfig.includeTutorialChunks ? 1 : 0;

        // Skills
        levelConfig.includedSkills.Sort(SortByKey);
        key += "P";
        foreach (Player.Skill skill in levelConfig.includedSkills)
        {
            if (skillDictionary.TryGetValue(skill, out int skillKey))
            {
                key += "s";
                key += skillKey;
            }
        }

        // Version
        key += "V";
        key += levelConfig.version;

        Debug.Log("Level Key generated: " + key);
        return key;
    }

    public static LevelConfig ReadKey(string key)
    {
        try
        {
            char[] delimiterChars = { 'S', 'I', 'M', 'T', 'P', 'V' };
            string[] configVals = key.Split(delimiterChars);

            int randomSeed = int.Parse(configVals[0]);
            float speed = float.Parse(configVals[1]) / 100;
            float maxIntensity = float.Parse(configVals[2]) / 100;
            int milestoneInterval = int.Parse(configVals[3]);
            bool includeTutorialChunks = int.Parse(configVals[4]) == 0 ? false : true;

            string[] includedSkillsKeys = configVals[5].Split('s');
            List<Player.Skill> includedSkills = new List<Player.Skill>();
            foreach (string skillKey in includedSkillsKeys)
            {
                if (int.TryParse(skillKey, out int skillKeyInt))
                {
                    Player.Skill skill = skillDictionary.FirstOrDefault(x => x.Value == skillKeyInt).Key;
                    includedSkills.Add(skill);
                }
            }
            includedSkills.Sort(SortByKey);

            int version = int.Parse(configVals[6]);

            LevelConfig levelConfig = new LevelConfig(randomSeed, speed, maxIntensity, milestoneInterval, includeTutorialChunks, includedSkills, version);
            Debug.Log("Level Config generated: " + levelConfig.ToString());
            return levelConfig;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return null;
        }
    }

    public static string FixSkillOrderInLevelKey(string levelKey)
    {
        return GenerateKey(ReadKey(levelKey));
    }

    private static int SortByKey(Player.Skill s1, Player.Skill s2)
    {
        if (skillDictionary.TryGetValue(s1, out int s1Key) && skillDictionary.TryGetValue(s2, out int s2Key))
        {
            return s1Key.CompareTo(s2Key);
        }
        return 0; // assume equal if one doesn't exist
    }

    public static string DefaultKey()
    {
        return "0S500I750M10T1Ps1s2s3s4V2";
    }
}
