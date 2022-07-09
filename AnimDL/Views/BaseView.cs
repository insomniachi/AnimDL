using Terminal.Gui;

namespace AnimDL.Views;

public abstract class BaseView<TViewModel>
{
	public Toplevel Top { get; set; } = Application.Top;
	public Window Win { get; set; } = new();
	public string Name { get; set; } = "";
	public TViewModel ViewModel { get; set; }

    public BaseView(TViewModel vm)
    {
		ViewModel = vm;
		Init(Application.Top, Colors.TopLevel);
	}

    public virtual void Init(Toplevel top, ColorScheme colorScheme)
    {
		Application.Init();

		Top = top;
		if (Top == null)
		{
			Top = Application.Top;
		}

		Win = new Window("{Name}")
		{
			X = 0,
			Y = 1,
			Width = Dim.Fill(),
			Height = Dim.Fill(),
			ColorScheme = colorScheme,
		};
		Top.Add(Win);
	}

	public virtual void Setup() { }
}

public abstract class BaseDialog<TViewModel> : Dialog
{
    public BaseDialog(string name, TViewModel vm ) : base(name) 
	{
		ViewModel = vm;
	}

	public TViewModel ViewModel { get; set; }

	public virtual void Setup() { }

	public void ShowDialog()
    {
		Setup();
		Application.Run(this);
    }
}
