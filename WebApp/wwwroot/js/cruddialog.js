function MsgBox(title, message) {
  /*
   var types = [BootstrapDialog.TYPE_DEFAULT,
                   BootstrapDialog.TYPE_INFO,
                   BootstrapDialog.TYPE_PRIMARY,
                   BootstrapDialog.TYPE_SUCCESS,
                   BootstrapDialog.TYPE_WARNING,
                   BootstrapDialog.TYPE_DANGER];
  */
  var type = BootstrapDialog.TYPE_DANGER;

  BootstrapDialog.show({
    title: title,
    message: message,
    type: type,
    draggable: true,
    closeByBackdrop: false,
    buttons: [{
      label: 'Close',
      action: function (dialogItself) {
        dialogItself.close();
      }
    }]
  });
}

function editFormatter(value, row) {
  return '<button id="btnEditRow" class="btn btn-xs btn-default" type="button"><i class="glyphicon glyphicon-pencil"></i></button>';
}

function delFormatter(value, row) {
  return '<button id="btnRemoveRow" class="btn btn-xs btn-default" type="button"><i class="glyphicon glyphicon-trash"></i></button>';
}

function crudFormatter(value, row) {
  var result = editFormatter(value, row) + ' ' + delFormatter(value, row);
  return result;
}


function getCrudName($table, id) {
  var row = $table.bootstrapTable('getRowByUniqueId', id);
  var fieldName = $table.attr('data-crudname');

  if (fieldName === undefined)
    fieldName = $table.attr('data-unique-id');

  var result = row[fieldName];

  return result;
}


function createUrl($table, id, verb) {
  var regEx = new RegExp('/load', "ig");
  var result = $table.attr('data-url').replace(regEx, '/' + verb)
  result = result + '/' + id;

  return result;
}


//  Fetch fresh row from controller
function updateGrid($table, resource) {
  var id = resource.Id;

  var $tr = $table.find("tr[data-uniqueid='" + id + "']");

  if ($tr.length === 0) {
    $table.bootstrapTable('insertRow', { index: 0, row: resource });
  }
  else
    $table.bootstrapTable('updateByUniqueId', { id: id, row: resource });

  $tr = $table.find("tr[data-uniqueid='" + id + "']");
  highLightRow($tr);
}


function editDialog($table, id) {
  var title = 'Add item';

  if (id === undefined) {
    id = '';
  }
  else {
    title = 'Edit <strong>' + getCrudName($table, id) + '</strong>';
  }

  var dlg = new BootstrapDialog({
    type: BootstrapDialog.TYPE_PRIMARY,
    title: title,
    message: 'Loading please wait...',
    draggable: true,
    closeByBackdrop: false,
    onshown: function (dialog) {
    },
    buttons: [{
      label: 'Cancel',
      action: function (sender) {
        sender.close();
      }
    },
    {
      id: 'btnsave',
      label: 'Save',
      icon: 'glyphicon glyphicon-ok',
      cssClass: 'btn-success',
      enabled: false,
      autospin: true,
      action: function (sender) {
        var $button = this;
        $button.disable();

        // find first form in dialog
        var formSelector = '#frmcrud';
        var $form = $(formSelector).get(0);

        $(formSelector).one('submit', function (e) {
          var formData = $(formSelector + ' :input').serialize();
          e.preventDefault();
          $.ajax({
            cache: false,
            context: { dialog: dlg, url: $form.action },
            url: $form.action,
            type: $form.method,
            data: formData,
            complete: function (msg) {
              $button.enable();
              $button.stopSpin();
              sender.setClosable(true);
            },
            error: function (result) {
              // Conflict error
              if (result.status == 409) {

                var msg = '';

                for (var i = 0; i < result.responseJSON.Errors.length; i++) {
                  msg = msg + result.responseJSON.Errors[i].Message + '<br>';
                }

                MsgBox('Multi User Error', msg);

                // refresh dialog content
                this.dialog.$modalBody.load(this.url);
              }
            },
            success: function (result) {
              // result can be an object with properties or an Html document
              if (result.Resource !== undefined) {
                // update underlying datagrid
                updateGrid($table, result.Resource);
              }

              // refresh grid and close dialog
              if (result.Errors !== undefined && result.Errors.length == 0) {
                // Close dialog
                sender.close();
              }
              else {
                // render dialog with new html and show validations errors
                this.dialog.$modalBody.html(result);
              }
            }
          });
        });
        sender.setClosable(false);
        $(formSelector).submit();
      }
    }
    ]
  });

  dlg.open();

  $.ajax({
    url: createUrl($table, id, 'edit'),
    type: 'get',
    cache: false,
    context: { dialog: dlg },
    error: function (ajaxRequest, status, errorThrown) {
      var errorMsg = extractErrorMessage(ajaxRequest.responseJSON);
      dlg.close();
      MsgBox(errorThrown, errorMsg);
    },
    success: function (data) {
      this.dialog.$modalBody.html(data);
      var btn = this.dialog.getButton('btnsave');
      btn.enable();
    }
  });
}


function extractErrorMessage(apiResult) {
  var msg = '';

  if (apiResult !== undefined) {
    for (var i = 0; i < apiResult.Errors.length; i++) {
      msg = msg + apiResult.Errors[i].Message + '<br>';
    }
  }
  return msg;
}

function deleteItem($table, id) {

  var dlg = new BootstrapDialog({
    type: BootstrapDialog.TYPE_WARNING,
    title: 'Please confirm delete',
    draggable: true,
    closeByBackdrop: false,
    buttons: [{
      label: 'Cancel',
      action: function (sender) {
        sender.close();
      }
    },
    {
      id: 'btndelete',
      icon: 'glyphicon glyphicon-trash',
      label: 'Delete',
      cssClass: 'btn-danger',
      autospin: true,
      enabled: false,
      hotkey: 13,
      action: function (sender) {
        var $button = this;
        $button.disable();
        sender.setClosable(false);
        $.ajax({
          url: createUrl($table, id, 'delete'),
          type: 'post',
          cache: false,
          context: { dialog: dlg, button: $button },
          error: function (ajaxRequest, status, errorThrown) {
            var errorMsg = extractErrorMessage(ajaxRequest.responseJSON);            
            MsgBox(errorThrown, errorMsg);
            dlg.close();
          },
          success: function () {
            this.button.stopSpin();
              $table.bootstrapTable('removeByUniqueId', id);
              dlg.close();
          }
        });
      }
    }
    ]
  });

  dlg.open();

  var btnDelete = dlg.getButton('btndelete');
  btnDelete.spin();

  $.ajax({
    url: createUrl($table, id, 'edit'),
    type: 'get',
    cache: false,
    context: { dialog: dlg },
    error: function (ajaxRequest, status, errorThrown) {
      var errorMsg = extractErrorMessage(ajaxRequest.responseJSON);      
      dlg.close();
      MsgBox(errorThrown, errorMsg);
    },
    success: function (data) {
      this.dialog.$modalBody.html(data);
      this.dialog.$modalBody.find(':input').attr('readonly', 'true');
      btnDelete.stopSpin();
      btnDelete.enable();
    }
  });

}

function highLightRow($tr) {
  $tr.addClass('highlight').siblings().removeClass('highlight');
}

function getId($element) {
  var result = $($element).closest('tr').data('uniqueid');
  return result;
}

function toggleEnabled(id, enabled) {
  var selector = '#' + id;

  if (enabled) {
    $(selector).removeAttr("disabled");
  }
  else {
    $(selector).attr('disabled', '');
  }
}

function getColumnNames(params, sender) {
  if (sender === undefined)
    sender = this;

  var columNames = '';
  var searchColumnNames = '';

  var columns = sender.columns[0];

  for (index = 0; index < columns.length; ++index) {
    var c = columns[index];

    if ((c.field !== undefined) && isNaN(c.field)) {
      columNames += c.field + '|';

      if ((c.searchable === undefined) || (c.searchable == true)) {
        searchColumnNames += c.field + '|';
      }
    }
  }

  params.fields = columNames;
  params.searchFields = searchColumnNames;

  return params;
}