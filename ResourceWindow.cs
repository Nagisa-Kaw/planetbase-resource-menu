using System.Collections.Generic;
using Planetbase;
using UnityEngine;

namespace PlanetbaseResourceMenu
{
    internal sealed class ResourceWindow : GuiWindow
    {
        private readonly List<RefBool> resourceHighlights = new List<RefBool>();
        private readonly GuiLabelItem summary = new GuiLabelItem("");
        private readonly GuiLabelItem status = new GuiLabelItem("Elige un recurso y ajusta la cantidad.");
        private ResourceType selected;
        private int amount = 10;
        private bool resumeOnClose;

        internal ResourceWindow() : base(new GuiLabelItem("Menú de recursos  ·  F8"))
        {
            List<ResourceType> types = TypeList<ResourceType, ResourceTypeList>.get();
            var resources = new GuiGridItem(3);
            foreach (ResourceType type in types)
            {
                var highlight = new RefBool();
                resourceHighlights.Add(highlight);
                var item = new GuiLabelItem(type.getName(), type.getIcon(), type.getName());
                item.addCheckbox(highlight, _ => Select(type));
                resources.addChild(item);
            }

            mRootItem.addChild(resources);
            mRootItem.addChild(summary);
            mRootItem.addChild(new GuiButtonItem("−10", _ => ChangeAmount(-10)));
            mRootItem.addChild(new GuiButtonItem("−1", _ => ChangeAmount(-1)));
            mRootItem.addChild(new GuiButtonItem("+1", _ => ChangeAmount(1)));
            mRootItem.addChild(new GuiButtonItem("+10", _ => ChangeAmount(10)));
            mRootItem.addChild(new GuiButtonItem("Agregar a la base", _ => Deliver()));
            mRootItem.addChild(status);

            Select(ResourceTypeList.MetalInstance);
        }

        public override float getWidth() => Mathf.Min(Screen.width * 0.9f, Screen.height * 0.82f);

        public override Exclusivity getExclusivity() => Exclusivity.Exclusive;

        public override void init()
        {
            TimeManager time = Singleton<TimeManager>.getInstance();
            resumeOnClose = !time.isPaused();
            if (resumeOnClose)
                time.pause();
        }

        public override void destroy()
        {
            if (resumeOnClose)
                Singleton<TimeManager>.getInstance().unpause();
        }

        public override void update() => RefreshSummary();

        private void Select(ResourceType type)
        {
            selected = type;
            var types = TypeList<ResourceType, ResourceTypeList>.get();
            for (int i = 0; i < resourceHighlights.Count; i++)
                resourceHighlights[i].set(types[i] == type);

            RefreshSummary();
        }

        private void ChangeAmount(int change)
        {
            amount = Mathf.Clamp(amount + change, 1, ResourceDelivery.MaxAmount);
            RefreshSummary();
        }

        private void RefreshSummary()
        {
            int count = Resource.getCountOfType(selected);
            summary.setText($"{selected.getName()}  ·  En la base {count}  ·  Cantidad {amount}");
        }

        private void Deliver()
        {
            status.setText(ResourceDelivery.Add(selected, amount));
            RefreshSummary();
        }
    }
}
