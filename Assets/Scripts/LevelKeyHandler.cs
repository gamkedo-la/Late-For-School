using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LevelKeyHandler
{
    public class LevelConfig
    {
        public int randomSeed;
        public float speed = 2.5f;
        public float maxIntensity = 10;
        public int milestoneInterval = 10;
        public bool includeTutorialChunks = true;
        public List<Player.Skill> includedSkills = new List<Player.Skill>();

        public LevelConfig(
            int randomSeed = 0,
            float speed = 2.5f,
            float maxIntensity = 10,
            int milestoneInterval = 10,
            bool includeTutorialChunks = true,
            List<Player.Skill> includedSkills = null)
        {
            this.randomSeed = randomSeed;
            this.speed = speed;
            this.maxIntensity = maxIntensity;
            this.milestoneInterval = milestoneInterval;
            this.includeTutorialChunks = includeTutorialChunks;
            this.includedSkills = includedSkills == null ?
                new List<Player.Skill> { Player.Skill.Dash, Player.Skill.Slide, Player.Skill.WallClimb, Player.Skill.WallJump } :
                includedSkills;
        }
    }

    public static Dictionary<Player.Skill, int> skillDictionary = new Dictionary<Player.Skill, int> {
        {Player.Skill.Dash, 1},
        {Player.Skill.Slide, 2},
        {Player.Skill.WallClimb, 3},
        {Player.Skill.WallJump, 4}
    };

    string GenerateKey(LevelConfig levelConfig)
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
        key += "P";
        foreach (Player.Skill skill in levelConfig.includedSkills)
        {
            if (skillDictionary.TryGetValue(skill, out int skillKey))
            {
                key += "s";
                key += skillKey;
            }
        }

        Debug.Log(key);
        return key;
    }

    LevelConfig ReadKey(string key)
    {
        char[] delimiterChars = { 'S', 'I', 'M', 'T', 'P' };
        string[] configVals = key.Split(delimiterChars);

        int randomSeed = int.Parse(configVals[0]);
        float speed = float.Parse(configVals[1]) / 100;
        float maxIntensity = float.Parse(configVals[2]) / 100;
        int milestoneInterval = int.Parse(configVals[3]);
        bool includeTutorialChunks = int.Parse(configVals[4]) == 0 ? false : true;

        string[]includedSkillsKeys = configVals[5].Split('s');
        List<Player.Skill> includedSkills = new List<Player.Skill>();
        foreach (string skillKey in includedSkillsKeys)
        {
            Player.Skill skill = skillDictionary.FirstOrDefault(x => x.Value == int.Parse(skillKey)).Key;
            includedSkills.Add(skill);
        }

        return new LevelConfig(randomSeed, speed, maxIntensity, milestoneInterval, includeTutorialChunks, includedSkills);
    }
}
