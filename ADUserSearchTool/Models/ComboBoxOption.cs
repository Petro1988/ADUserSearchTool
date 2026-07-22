namespace ADUserSearchTool.Models
{
    public class ComboBoxOption<T>
    {
        public T Value { get; }

        public string DisplayName { get; }

        public ComboBoxOption(T value, string displayName)
        {
            Value = value;
            DisplayName = displayName;
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
