namespace FclEx.YamlDotNet;

public class YamlMappingNodeExtensionsTests
{
    public static readonly string Yaml = """
                                         ---
                                         receipt:     Oz-Ware Purchase Invoice
                                         date:        2012-08-06
                                         customer:
                                             first_name:   Dorothy
                                             family_name:  Gale
                                         
                                         items:
                                             - part_no:   A4786
                                               descrip:   Water Bucket (Filled)
                                               price:     1.47
                                               quantity:  4
                                         
                                             - part_no:   E1628
                                               descrip:   High Heeled "Ruby" Slippers
                                               size:      8
                                               price:     133.7
                                               quantity:  1
                                         
                                         bill-to:  &id001
                                             street: |
                                                     123 Tornado Alley
                                                     Suite 16
                                             city:   East Centerville
                                             state:  KS
                                         
                                         ship-to:  *id001
                                         
                                         specialDelivery:  >
                                             Follow the Yellow Brick
                                             Road to the Emerald City.
                                             Pay no attention to the
                                             man behind the curtain.
                                         ...
                                         """;

    private static YamlMappingNode ReadYaml()
    {
        using var input = new StringReader(Yaml);
        var yaml = new YamlStream();
        yaml.Load(input);
        var root = yaml.Documents[0].RootNode.CastTo<YamlMappingNode>();
        return root;
    }

    [Fact]
    public void Child_Test()
    {
        var root = ReadYaml();

        {
            var child = root.Child<YamlScalarNode>("receipt");
            Assert.NotNull(child);
            Assert.Equal("Oz-Ware Purchase Invoice", child.Value);
        }

        {
            var child = root.Child<YamlScalarNode>("non-exist");
            Assert.Null(child);
        }
    }

    [Fact]
    public void RequiredChild_Test()
    {
        var root = ReadYaml();

        {
            var child = root.RequiredChild<YamlScalarNode>("date");
            Assert.NotNull(child);
            Assert.Equal("2012-08-06", child.Value);
        }

        {
            Assert.Throws<KeyNotFoundException>(() => root.RequiredChild<YamlScalarNode>("non-exist"));
        }
    }

    [Fact]
    public void Children_Test()
    {
        var root = ReadYaml();
        var children = root.Children().ToArray();
        string[] keys = ["receipt", "date", "customer", "items", "bill-to", "ship-to", "specialDelivery"];
        Assert.Equal(keys, children.Select(m => m.Key.Value!).ToArray());
    }

    [Fact]
    public void RemoveChild_Test()
    {
        var root = ReadYaml();
        Assert.Contains("receipt", root.Children.Keys);
        root.RemoveChild("receipt");
        Assert.DoesNotContain("receipt", root.Children.Keys);
    }
}