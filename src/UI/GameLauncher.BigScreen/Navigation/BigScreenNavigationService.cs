using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace GameLauncher.BigScreen.Navigation
{
    /// <summary>
    /// Servicio de navegación basado en stack para BigScreen.
    /// Permite navegar entre vistas con historial (push/pop).
    /// </summary>
    public class BigScreenNavigationService
    {
        private readonly Stack<NavigationEntry> _navigationStack = new();
        private readonly Frame _frame;

        // Eventos
        public event EventHandler<NavigatedEventArgs>? Navigated;
        public event EventHandler<NavigatingEventArgs>? Navigating;

        public bool CanGoBack => _navigationStack.Count > 1;
        public NavigationEntry? CurrentView => _navigationStack.Count > 0 ? _navigationStack.Peek() : null;

        public BigScreenNavigationService(Frame frame)
        {
            _frame = frame ?? throw new ArgumentNullException(nameof(frame));
        }

        /// <summary>
        /// Navega a una nueva vista empujándola al stack.
        /// </summary>
        public void NavigateTo(Type viewType, object? parameter = null)
        {
            if (viewType == null)
                throw new ArgumentNullException(nameof(viewType));

            if (!typeof(Page).IsAssignableFrom(viewType))
                throw new ArgumentException($"{viewType.Name} debe heredar de Page", nameof(viewType));

            // Evento pre-navegación
            var navigatingArgs = new NavigatingEventArgs(viewType, parameter);
            Navigating?.Invoke(this, navigatingArgs);

            if (navigatingArgs.Cancel)
                return;

            try
            {
                // Crear instancia de la vista
                var view = Activator.CreateInstance(viewType) as Page;

                if (view == null)
                    throw new InvalidOperationException($"No se pudo crear instancia de {viewType.Name}");

                // Si la vista tiene un parámetro, pasarlo al DataContext si implementa una interfaz
                if (parameter != null && view.DataContext is INavigationAware aware)
                {
                    aware.OnNavigatedTo(parameter);
                }

                // Empujar al stack
                var entry = new NavigationEntry(viewType, view, parameter);
                _navigationStack.Push(entry);

                // Navegar en el Frame
                _frame.Navigate(view);

                // Evento post-navegación
                Navigated?.Invoke(this, new NavigatedEventArgs(entry));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al navegar a {viewType.Name}", ex);
            }
        }

        /// <summary>
        /// Navega a una nueva vista con un ViewModel específico.
        /// </summary>
        public void NavigateTo<TView>(object? viewModel = null, object? parameter = null) where TView : Page
        {
            var viewType = typeof(TView);

            // Evento pre-navegación
            var navigatingArgs = new NavigatingEventArgs(viewType, parameter);
            Navigating?.Invoke(this, navigatingArgs);

            if (navigatingArgs.Cancel)
                return;

            try
            {
                // Crear instancia de la vista
                var view = Activator.CreateInstance<TView>();

                // Asignar ViewModel si se proporcionó
                if (viewModel != null)
                {
                    view.DataContext = viewModel;
                }

                // Si hay parámetro y el ViewModel implementa INavigationAware
                if (parameter != null && viewModel is INavigationAware aware)
                {
                    aware.OnNavigatedTo(parameter);
                }

                // Empujar al stack
                var entry = new NavigationEntry(viewType, view, parameter);
                _navigationStack.Push(entry);

                // Navegar en el Frame
                _frame.Navigate(view);

                // Evento post-navegación
                Navigated?.Invoke(this, new NavigatedEventArgs(entry));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al navegar a {viewType.Name}", ex);
            }
        }

        /// <summary>
        /// Vuelve a la vista anterior (pop del stack).
        /// </summary>
        public bool GoBack()
        {
            if (!CanGoBack)
                return false;

            try
            {
                // Pop de la vista actual
                var currentEntry = _navigationStack.Pop();

                // Notificar a la vista que está saliendo
                if (currentEntry.View?.DataContext is INavigationAware currentAware)
                {
                    currentAware.OnNavigatedFrom();
                }

                // Peek de la vista anterior
                var previousEntry = _navigationStack.Peek();

                // Navegar a la vista anterior
                _frame.Navigate(previousEntry.View);

                // Notificar a la vista anterior que está regresando
                if (previousEntry.View?.DataContext is INavigationAware previousAware)
                {
                    previousAware.OnNavigatedBack(previousEntry.Parameter);
                }

                // Evento de navegación
                Navigated?.Invoke(this, new NavigatedEventArgs(previousEntry));

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Limpia el stack de navegación y navega a la vista inicial.
        /// </summary>
        public void NavigateToRoot(Type viewType, object? parameter = null)
        {
            // Limpiar stack
            _navigationStack.Clear();

            // Navegar a la raíz
            NavigateTo(viewType, parameter);
        }

        /// <summary>
        /// Limpia completamente el historial de navegación.
        /// </summary>
        public void ClearHistory()
        {
            // Notificar a todas las vistas que están saliendo
            while (_navigationStack.Count > 0)
            {
                var entry = _navigationStack.Pop();
                if (entry.View?.DataContext is INavigationAware aware)
                {
                    aware.OnNavigatedFrom();
                }
            }
        }
    }

    /// <summary>
    /// Entrada en el stack de navegación.
    /// </summary>
    public class NavigationEntry
    {
        public Type ViewType { get; }
        public Page? View { get; }
        public object? Parameter { get; }
        public DateTime NavigatedAt { get; }

        public NavigationEntry(Type viewType, Page? view, object? parameter)
        {
            ViewType = viewType ?? throw new ArgumentNullException(nameof(viewType));
            View = view;
            Parameter = parameter;
            NavigatedAt = DateTime.Now;
        }
    }

    /// <summary>
    /// Argumentos del evento Navigated.
    /// </summary>
    public class NavigatedEventArgs : EventArgs
    {
        public NavigationEntry Entry { get; }

        public NavigatedEventArgs(NavigationEntry entry)
        {
            Entry = entry ?? throw new ArgumentNullException(nameof(entry));
        }
    }

    /// <summary>
    /// Argumentos del evento Navigating (antes de navegar).
    /// </summary>
    public class NavigatingEventArgs : EventArgs
    {
        public Type ViewType { get; }
        public object? Parameter { get; }
        public bool Cancel { get; set; }

        public NavigatingEventArgs(Type viewType, object? parameter)
        {
            ViewType = viewType ?? throw new ArgumentNullException(nameof(viewType));
            Parameter = parameter;
            Cancel = false;
        }
    }

    /// <summary>
    /// Interfaz para ViewModels que necesitan ser notificados de eventos de navegación.
    /// </summary>
    public interface INavigationAware
    {
        /// <summary>
        /// Se llama cuando se navega A esta vista.
        /// </summary>
        void OnNavigatedTo(object? parameter);

        /// <summary>
        /// Se llama cuando se navega DESDE esta vista.
        /// </summary>
        void OnNavigatedFrom();

        /// <summary>
        /// Se llama cuando se regresa a esta vista con GoBack().
        /// </summary>
        void OnNavigatedBack(object? parameter);
    }
}
