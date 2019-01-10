using System.Linq;

namespace Forum.Helpers {
  public static class StringHelper {
    public static string FirstValidString(params string[] strings) {
      return strings.FirstOrDefault(s => !string.IsNullOrEmpty(s));
    }
  }
}