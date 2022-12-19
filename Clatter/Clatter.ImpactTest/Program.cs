using Clatter.Core;


namespace Clatter.ImpactTest
{
    public class Program
    {
        private static void Main(string[] args)
        {
            Creator.SetPrimaryObject(new AudioObjectData(0, ImpactMaterialSized.glass_1, 0.2f, 0.2f, 1));
            Creator.SetSecondaryObject(new AudioObjectData(1, ImpactMaterialSized.stone_4, 0.5f, 0.1f, 100));
            Creator.SetRandom(0);
            Creator.WriteImpact(1, true, "out.wav");
        }
    }
}