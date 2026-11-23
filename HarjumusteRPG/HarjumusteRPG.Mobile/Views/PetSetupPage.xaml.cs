using HarjumusteRPG.Mobile.ViewModels;
using Microsoft.Maui.Controls.Shapes;

namespace HarjumusteRPG.Mobile.Views;

public partial class PetSetupPage : ContentPage
{
    private readonly VerticalStackLayout _column;

    public PetSetupPage(PetSetupViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
        BackgroundColor = Palette.Bg;
        Shell.SetNavBarIsVisible(this, false);
        this.SetBinding(TitleProperty, nameof(PetSetupViewModel.Title));

        var heading = Pixel.Heading("VALI OMA LEMMIK", 16, center: true);
        var hint = Pixel.Body("Vali loom ja anna talle nimi. Ta kasvab koos sinu harjumustega!", 16,
            color: Palette.Sub, center: true);

        // --- grid of animals; the pink one is selected ---
        var grid = new FlexLayout
        {
            Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap,
            JustifyContent = Microsoft.Maui.Layouts.FlexJustify.Center
        };
        BindableLayout.SetItemsSource(grid, viewModel.Pets);
        BindableLayout.SetItemTemplate(grid, new DataTemplate(CreatePet));

        var name = Pixel.Field("Lemmiku nimi (nt Muri)", nameof(PetSetupViewModel.PetName),
            returnType: ReturnType.Done, returnCommandPath: nameof(PetSetupViewModel.SaveCommand));

        var error = Pixel.ErrorLabel(nameof(PetSetupViewModel.ErrorMessage), nameof(PetSetupViewModel.HasError));

        _column = new VerticalStackLayout
        {
            Padding = new Thickness(24, 28),
            Spacing = 16,
            HorizontalOptions = LayoutOptions.Center,
            Children =
            {
                heading,
                hint,
                grid,
                name,
                error,
                Pixel.PrimaryButton("Alusta", nameof(PetSetupViewModel.SaveCommand))
            }
        };

        Content = new ScrollView { Content = _column };
    }

    // One animal: picture and name. Tap to select.
    private View CreatePet()
    {
        var image = new Image
        {
            WidthRequest = 72,
            HeightRequest = 72,
            Aspect = Aspect.AspectFit,
            HorizontalOptions = LayoutOptions.Center
        };
        image.SetBinding(Image.SourceProperty, nameof(PetChoice.ImageFile));

        var label = Pixel.Body(string.Empty, 13, bold: true, center: true);
        label.SetBinding(Label.TextProperty, nameof(PetChoice.Label));

        var cell = new Border
        {
            StrokeThickness = 3,
            Stroke = Palette.Ink,
            StrokeShape = new Rectangle(),
            WidthRequest = 96,
            Padding = new Thickness(6),
            Margin = new Thickness(0, 0, 8, 8),
            Content = new VerticalStackLayout { Spacing = 4, Children = { image, label } }
        };
        cell.SetBinding(VisualElement.BackgroundColorProperty,
            new Binding(nameof(PetChoice.IsSelected), converter: new BoolToColorConverter(Palette.Hero, Palette.Card)));

        var tap = new TapGestureRecognizer();
        tap.SetBinding(TapGestureRecognizer.CommandProperty,
            new Binding(nameof(PetSetupViewModel.SelectCommand), source: BindingContext));
        tap.SetBinding(TapGestureRecognizer.CommandParameterProperty, Binding.SelfPath);
        cell.GestureRecognizers.Add(tap);

        return cell;
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (width > 0)
            _column.WidthRequest = Math.Min(width, 420);
    }
}