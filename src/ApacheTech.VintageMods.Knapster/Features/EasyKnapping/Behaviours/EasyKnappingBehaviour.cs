using Vintagestory.API.Config;
using Vintagestory.API.Util;

namespace ApacheTech.VintageMods.Knapster.Features.EasyKnapping.Behaviours
{
    public class EasyKnappingBehaviour : CollectibleBehavior
    {
        private List<SkillItem> _toolModes;

        public EasyKnappingBehaviour(CollectibleObject collObj) : base(collObj)
        {

        }

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            _toolModes = ObjectCacheUtil.GetOrCreate(api, "easyKnappingToolModes", () =>
            {
                var capi = api as ICoreClientAPI;
                return new List<SkillItem>
                {
                    new SkillItem
                    {
                        Code = new AssetLocation("1size"),
                        Name = Lang.Get("1x1")
                    }.WithIcon(capi, ItemClay.Drawcreate1_svg),
                    new SkillItem
                    {
                        Code = new AssetLocation("auto"),
                        Name = LangEx.FeatureString("Knapster", "AutoComplete")
                    }.WithIcon(capi, ApiEx.Client.Gui.Icons.Drawfloodfill_svg)
                };
            });
        }

        public override int GetToolMode(ItemSlot slot, IPlayer byPlayer, BlockSelection blockSel)
        {
            return Math.Min(_toolModes.Count - 1, slot.Itemstack.Attributes.GetInt("toolMode"));
        }

        public override void SetToolMode(ItemSlot slot, IPlayer byPlayer, BlockSelection blockSel, int toolMode)
        {
            slot.Itemstack.Attributes.SetInt("toolMode", toolMode);
        }

        public override SkillItem[] GetToolModes(ItemSlot slot, IClientPlayer forPlayer, BlockSelection blockSel)
        {
            return _toolModes.ToArray();
        }
    }
}