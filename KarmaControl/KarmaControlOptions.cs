using Menu.Remix.MixedUI;

namespace KarmaControl;
public class KarmaControlOptions : OptionInterface
{
    public readonly Configurable<bool> Hide;
    public KarmaControlOptions()
    {
        this.Hide = this.config.Bind("KarmaControl_Bool_Hide", false);
    }

    public override void Initialize()
    {
        base.Initialize();
        
        OpTab opTab = new OpTab(this, "Options");
        Tabs = new OpTab[]
        {
            opTab
        };
        
        UIelement[] elements = new UIelement[]
        {
            new OpLabel(10f, 550f, "Options", true),
            new OpLabel(40f, 520f, "Hide karma control menu?"),
            new OpCheckBox(this.Hide, 10f, 520f)
        };
        
        opTab.AddItems(elements);
    }
}