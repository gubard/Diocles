using Diocles.Services;
using Hestia.Contract.Models;
using Inanna.Controls;

namespace Diocles.Controls;

public abstract class ToDoDropUserControl
    : DropUserControl<
        IToDoUiService,
        HestiaGetRequest,
        HestiaPostRequest,
        HestiaGetResponse,
        HestiaPostResponse,
        EditToDos
    >;
