using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodAI.Arnold.Graphics
{
    public abstract class CompositeLookupModelBase<TKey, TModel> : ModelBase, ICompositeModel, IEnumerable<TModel>
        where TModel : IModel
    {
        protected readonly IDictionary<TKey, TModel> Children = new Dictionary<TKey, TModel>();

        public IEnumerable<IModel> GenericModels => Children.Values as IEnumerable<IModel>;
        public IEnumerator<TModel> GetEnumerator()
        {
            return Children.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public TModel this[TKey key]
        {
            get { return Children[key]; }
            set
            {
                value.Owner = this;
                Children[key] = value;
            }
        }

        public bool Remove(TKey key)
        {
            Children[key].Owner = null;
            return Children.Remove(key);
        }

        public override void Update(float elapsedMs)
        {
            base.Update(elapsedMs);

            foreach (var child in Children.Values)
                child.Update(elapsedMs);
        }
    }

    public class CompositeLookupModel<TKey, TModel> : CompositeLookupModelBase<TKey, TModel> where TModel : IModel
    {
        protected override void UpdateModel(float elapsedMs)
        {
        }

        protected override void RenderModel(float elapsedMs)
        {
        }
    }
}
