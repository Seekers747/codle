using Microsoft.AspNetCore.Components;

namespace MiniGameVisual.Components.Elements
{
  public partial class GameGrid
  {
    [Parameter] public char[,] Grid { get; set; } = new char[6, 5];
    [Parameter] public string[,] GridStyles { get; set; } = new string[6, 5];

    private string GetCellClass(int row, int col)
    {
      var style = GridStyles[row, col];
      return style switch
      {
        "typed" => "cell-typed",
        "correct" => "cell-correct",
        "present" => "cell-present",
        "absent" => "cell-absent",
        _ => "cell-empty"
      };
    }
  }
}
