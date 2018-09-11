$(function () {
    var AllStrategies;
    var CurrentSecurity;
    $.ajax({
        async: false,
        url: "matrix/Main/GetAvailableStrategies",
        type: 'GET',
        success: function (data) {
            AllStrategies = data;
        }
    })

    $('#btnConnection').click(function () {
        $.getJSON("/Main/LoginAccount", function (data) {
            location.reload();
        })
    })
    $('#btnDisConnection').click(function () {
        $.getJSON("/Main/LogoutAccount", function (data) {
            location.reload();
        })
    })
    $('#btnSaveEditModal').click(function () {
        CurrentSecurity.strategyName = $('#SelectStrategy').find(":selected").text();
        var raw = {
            "TickerName": CurrentSecurity.tickerName,
            "AccountName": CurrentSecurity.accountName,
            "IBStrategy": CurrentSecurity.strategyName
        }
        $.ajax({
            async: false,
            url: "/Main/UpdateSecurity",
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(raw),
            success: function () {
                alert('Save Succesfully');
                $('#modalEditSecurity').modal('toggle');
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                alert('Save Failed\n' + errorThrown);
            }
        });
    });
    $('.tblStrategy').DataTable();
    $('.tblSecType').DataTable();
    function format(d) {
        var col = ['Ticker', 'Type', 'Pos', 'Strike', 'Moneyness', 'Premium', 'MV', 'Delta', 'Gamma', 'Theta', 'Vega', 'DailyPL', 'UnPL', 'RzPL'];
        var result = '<table class="display table table-condensed" style="table-layout:fixed"><thead><tr>';
        $.each(col, function (i, item) {
            result += '<th>' + item + '</th>';
        });
        result += '</tr></thead><tbody>';
        $.ajax({
            async: false,
            url: "/Main/GetOptionLegs/",
            type: 'GET',
            contentType: 'application/json',
            data: { tag: d[1] },
            success: function (data) {
                $.each(data, function (i, item) {
                    result +=
                        '<tr>'
                        + '<td>' + item.tickerName + '</td>'
                        + '<td>' + item.contract.Right + '</td>'
                        + '<td>' + item.position + '</td>'
                        + '<td>' + item.contract.Strike + '</td>'
                        + '<td>' + Math.round(item.contract.Strike / item.Underlying * 100) + '</td>'
                        + '<td>' + Math.round(item.premium) + '</td>'
                        + '<td>' + Math.round(item.marketValue) + '</td>'
                        + '<td>' + Math.round(item.Delta, 4) + '</td>'
                        + '<td>' + Math.round(item.Gamma, 4) + '</td>'
                        + '<td>' + Math.round(item.Theta, 4) + '</td>'
                        + '<td>' + Math.round(item.Vega, 4) + '</td>'
                        + '<td>' + item.DailyPNL + '</td>'
                        + '<td>' + item.unrealizedPNL + '</td>'
                        + '<td>' + item.realizedPNL + '</td>'
                        + '</tr>';
                })
            }
        })
        result += '</tbody></table>';
        return result;
    };

    var tblSecTypeOption = $('#tblSecTypeOption').DataTable(
        {
            "columns": [
                {
                    "className": 'details-control',
                    "orderable": false,
                    "data": null,
                    "defaultContent": ''
                },
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
            ]
        });
    $('#tblSecTypeOption tbody').on('click', 'td.details-control', function () {
        var tr = $(this).closest('tr');
        var row = tblSecTypeOption.row(tr);

        if (row.child.isShown()) {
            // This row is already open - close it
            row.child.hide();
            tr.removeClass('shown');
        }
        else {
            // Open this row
            var tb = format(row.data());
            row.child(tb).show();
            tr.addClass('shown');
        }
    });
    $('#modalEditSecurity').on("show.bs.modal", function (e) {
        $('#SelectStrategy').find('option').remove();
        CurrentSecurity = $(e.relatedTarget).data('position');

        $('#modalheader').html(CurrentSecurity.tickerName);
        $.each(AllStrategies, function (i, item) {
            $('#SelectStrategy').append($('<option>', {
                value: item,
                text: item,
                selected: item == CurrentSecurity.strategyName
            }))
        })
    }).on("hide.bs.modal", function () {
        location.reload();
    });
})