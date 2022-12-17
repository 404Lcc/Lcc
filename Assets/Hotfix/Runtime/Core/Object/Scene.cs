using LccModel;

namespace LccHotfix
{
    public static class SceneFactory
    {
        public static Scene CreateScene(long id, long instanceId, string name, AObjectBase parent = null)
        {
            Scene scene = new Scene(id, instanceId, name, parent);

            return scene;
        }
        public static Scene CreateScene(string name, AObjectBase parent = null)
        {
            Scene scene = new Scene(IdUtil.GenerateId(), IdUtil.GenerateInstanceId(), name, parent);

            return scene;
        }
    }
    public class Scene : AObjectBase
    {
        public string name;

        public Scene(long id, long instanceId, string name, AObjectBase parent)
        {
            this.Id = id;
            this.InstanceId = instanceId;

            this.name = name;

            this.Parent = parent;
            this.Domain = this;

        }
        public new AObjectBase Parent
        {
            get
            {
                return this._parent;
            }
            private set
            {
                if (value == null)
                {
                    return;
                }

                this._parent = value;
                this._parent.Children.Add(this.Id, this);
            }
        }
        public new AObjectBase Domain
        {
            get => this._domain;
            private set => this._domain = value;
        }
    }
}