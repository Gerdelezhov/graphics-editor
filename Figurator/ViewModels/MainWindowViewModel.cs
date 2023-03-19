using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Figurator.Models;
using Figurator.Views;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;

namespace Figurator.ViewModels {
    public class Log {
        public static MainWindowViewModel? Mwvm { private get; set; }
        public static void Write(string message) {
            if (Mwvm != null) Mwvm.Logg += message;
        }
    }

    public class MainWindowViewModel: ViewModelBase {
        private UserControl content;
        private int shaper_n = 0;
        private readonly Mapper map;
        private readonly Canvas canv;

        private readonly UserControl[] contentArray = new UserControl[] {
            new Shape1_UserControl(),
            new Shape2_UserControl(),
            new Shape3_UserControl(),
            new Shape4_UserControl(),
            new Shape5_UserControl(),
            new Shape6_UserControl()
        };

        private string log = "";
        public string Logg { get => log; set => this.RaiseAndSetIfChanged(ref log, value + "\n"); }

        private bool is_enabled = true;
        private IBrush add_color = Brushes.White;
        public IBrush AddColor { get => add_color; set => this.RaiseAndSetIfChanged(ref add_color, value); }

        private Shape? animated_part = null;
        private void Update() {
            bool valid = map.ValidInput();
            bool valid2 = map.ValidName();

            is_enabled = valid;

            AddColor = valid ? valid2 ? Brushes.Lime : Brushes.Yellow : Brushes.Pink;
            ShapeNameColor = valid2 ? Brushes.Lime : Brushes.Yellow;

            if (map.newName != null) {
                var name = map.newName;
                map.newName = null; 
                ShapeName = name;
            }

            if (animated_part != null) {
                canv.Children.Remove(animated_part);
                animated_part = null;
            }

            if (valid) {
                Shape? newy = map.Create(true);
                if (newy != null) {
                    newy.Classes.Add("anim");
                    canv.Children.Add(newy);
                    animated_part = newy;
                }
            }

            var select = map.select_shaper;
            if (select != -1) {
                map.select_shaper = -1;
                if (select == shaper_n) SelectedShaper = select == 0 ? 1 : 0; 
                SelectedShaper = select;
            }
        }
        private static void Update(object? inst) {
            if (inst != null && inst is MainWindowViewModel @mwvm) @mwvm.Update();
        }

        public MainWindowViewModel(MainWindow mw) {
            Log.Mwvm = this;
            content = contentArray[0];
            map = new(Update, this);
            canv = mw.Find<Canvas>("canvas");
            Update();

            Add = ReactiveCommand.Create<Unit, Unit>(_ => { FuncAdd(); return new Unit(); });
            Clear = ReactiveCommand.Create<Unit, Unit>(_ => { FuncClear(); return new Unit(); });
            Export = ReactiveCommand.Create<string, Unit>(n => { FuncExport(n); return new Unit(); });
            Import = ReactiveCommand.Create<string, Unit>(n => { FuncImport(n); return new Unit(); });
        }

        public int SelectedShaper {
            get => shaper_n;
            set { this.RaiseAndSetIfChanged(ref shaper_n, value); map.ChangeFigure(value); Content = contentArray[value]; }
        }

        public UserControl Content {
            get => content;
            set => this.RaiseAndSetIfChanged(ref content, value);
        }

        private void FuncAdd() {
            if (!is_enabled) return;
          
            Shape? newy = map.Create(false);
            if (newy == null) return;

            canv.Children.Add(newy);
            Update();
        }
        private void FuncClear() => map.Clear();
        private void FuncExport(string Type) {
            if (Type == "PNG") {
                ServiceVisible = false;
                if (animated_part != null) animated_part.IsVisible = false;

                try {
                    Utils.RenderToFile(canv, "../../../Export.png");
                } catch (Exception e) {
                    Log.Write("Ошибка экспорта PNG: " + e);
                }

                ServiceVisible = true;
                if (animated_part != null) animated_part.IsVisible = true;
            } else map.Export(Type == "XML");
        }
        private void FuncImport(string Type) {
            Shape[]? beginners = map.Import(Type == "XML");
            if (beginners == null) return;

            foreach (var beginner in beginners) canv.Children.Add(beginner);
            Update();
        }

        public ReactiveCommand<Unit, Unit> Add { get; }
        public ReactiveCommand<Unit, Unit> Clear { get; }
        public ReactiveCommand<string, Unit> Export { get; }
        public ReactiveCommand<string, Unit> Import { get; }

        public void ShapeTap(string name) {
            var item = map.ShapeTap(name);
            this.RaiseAndSetIfChanged(ref cur_shape, item, nameof(SelectedShape));
        }


        private IBrush nameColor = Brushes.White;
        public string ShapeName { get => map.shapeName; set { this.RaiseAndSetIfChanged(ref map.shapeName, value); Update(); } }
        public IBrush ShapeNameColor { get => nameColor; set => this.RaiseAndSetIfChanged(ref nameColor, value); }

        public string ShapeColor { get => map.shapeColor; set { this.RaiseAndSetIfChanged(ref map.shapeColor, value); Update(); } }
        public string ShapeFillColor { get => map.shapeFillColor; set { this.RaiseAndSetIfChanged(ref map.shapeFillColor, value); Update(); } }
        public int ShapeThickness { get => map.shapeThickness; set { this.RaiseAndSetIfChanged(ref map.shapeThickness, value); Update(); } }

        public SafeNum ShapeWidth => map.shapeWidth;
        public SafeNum ShapeHeight => map.shapeHeight; 
        public SafeNum ShapeHorizDiagonal => map.shapeHorizDiagonal;
        public SafeNum ShapeVertDiagonal => map.shapeVertDiagonal;

        public SafePoint ShapeStartDot => map.shapeStartDot;
        public SafePoint ShapeEndDot => map.shapeEndDot;
        public SafePoint ShapeCenterDot => map.shapeCenterDot;
        public SafePoints ShapeDots => map.shapeDots;

        public SafeGeometry ShapeCommands => map.shapeCommands;

        private readonly static string[] colors = new[] {
            "Yellow", "Blue", "Green", "Red",
            "Orange", "Brown", "Pink", "Aqua",
            "Lime",
            "White", "LightGray", "DarkGray", "Black"
        };
        public static string[] ColorsArr { get => colors; }

        public ObservableCollection<ShapeListBoxItem> Shapes { get => map.shapes; }

        private bool service_visible = true;
        public bool ServiceVisible { get => service_visible; set => this.RaiseAndSetIfChanged(ref service_visible, value); }

        private ShapeListBoxItem? cur_shape;
        public ShapeListBoxItem? SelectedShape { get => cur_shape;
            set { this.RaiseAndSetIfChanged(ref cur_shape, value); map.Select(value); }
        }
    }
}