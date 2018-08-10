using System.Threading.Tasks;

namespace Common
{
    public interface IInitializable
    {
        Task Init(Context context);
    }
}