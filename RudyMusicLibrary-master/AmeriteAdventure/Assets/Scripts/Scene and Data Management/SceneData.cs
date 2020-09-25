[System.Serializable]
public class SceneData
{
    public float hp;
    public int SceneNum;

    public SceneData()
    {
        hp = 4f;
        SceneNum = 1;
    }

    public SceneData(float health)
    {
        hp = health;
        SceneNum = 1;
    }

    public SceneData(float health, int numScene)
    {
        hp = health;
        SceneNum = numScene;
    }
}
