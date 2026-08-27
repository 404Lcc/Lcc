namespace LccHotfix
{
    public struct EntityVarCfg
    {
        public string VarKey { get; private set; }

        public EntityVarCfg(string varName)
        {
            VarKey = varName;
        }

        public LogicEntity GetEntity(CustomNode node, bool logError = true)
        {
            var entityVarKey = VarKey;
            return GetEntity(node, entityVarKey, logError);
        }

        public static LogicEntity GetEntity(CustomNode node, string entityVarKey, bool logError = true)
        {
            var entityRef = node.GetVar<LogicEntity>(entityVarKey);
            if (entityRef != null)
            {
                return entityRef;
            }

            var entityID = node.GetVar<long>(entityVarKey);
            if (entityID == 0)
            {
                if (logError)
                {
                    CLogger.LogError(node, $"EntityCfg.GetEntity entityID=0, VarKey={entityVarKey}");
                }

                return null;
            }

            var world = node.GetLogicWorld();
            if (world == null)
            {
                CLogger.LogError(node, $"EntityCfg.GetEntity world != null, VarKey={entityVarKey}");
                return null;
            }

            var entity = world.GetEntityWithComID(entityID);
            if (logError && entity == null)
            {
                CLogger.LogError(node, $"EntityCfg.GetEntity entity == null, entityID={entityID}, VarKey={entityVarKey}");
            }

            return entity;
        }
    }
}