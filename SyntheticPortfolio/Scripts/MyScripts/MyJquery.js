$(function () {
    var MainPath = "/Matrix/Main";
    var AllStrategies;
    var CurrentSecurity;
    $.ajax({
        async: false,
        url: MainPath + "/GetAvailableStrategies",
        type: 'GET',
        success: function (data) {
            AllStrategies = data;
        }
    })

    $('#btnConnection').click(function () {
        $.getJSON(MainPath + "/LoginAccount", function (data) {
            location.reload();
        })
    })
    $('#btnDisConnection').click(function () {
        $.getJSON(MainPath + "/LogoutAccount", function (data) {
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
            url: MainPath + "/UpdateSecurity",
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
        var col = ['Ticker', 'DailyPL', 'Pos', 'Bid', 'Ask', 'Cost', 'Type', 'Strike', 'S0', 'Moneyness', 'Premium', 'MV', 'IV', 'Delta', 'Gamma', 'Theta', 'Vega', 'UnPL', 'RzPL'];
        var result = '<table class="table table-condensed table-bordered " style="table-layout:fixed;font-size:11px;font-weight:300;"><thead><tr>';
        $.each(col, function (i, item) {
            result += '<th>' + item + '</th>';
        });
        result += '</tr></thead><tbody>';
        $.ajax({
            async: false,
            url: MainPath + "/GetOptionLegs/",
            type: 'GET',
            contentType: 'application/json',
            data: { tag: d[1] },
            success: function (data) {
                $.each(data, function (i, item) {
                    result +=
                        '<tr>'
                        + '<td>' + item.tickerName + '</td>'
                        + '<td>' + Math.round(item.DailyPNL) + '</td>'
                        + '<td>' + item.position + '</td>'
                        + '<td>' + item.Bid + '</td>'
                        + '<td>' + item.Ask + '</td>'
                        + '<td>' + (item.averageCost / item.contract.Multiplier).toFixed(2) + '</td>'
                        + '<td>' + item.contract.Right + '</td>'
                        + '<td>' + item.contract.Strike + '</td>'
                        + '<td>' + item.Underlying.toFixed(2) + '</td>'
                        + '<td>' + (item.contract.Strike / item.Underlying * 100).toFixed(2) + '</td>'
                        + '<td>' + Math.round(item.premium) + '</td>'
                        + '<td>' + Math.round(item.marketValue) + '</td>'
                        + '<td>' + item.ImpliedVol.toFixed(2) + '</td>'
                        + '<td>' + Math.round(item.Delta) + '</td>'
                        + '<td>' + Math.round(item.Gamma) + '</td>'
                        + '<td>' + Math.round(item.Theta) + '</td>'
                        + '<td>' + Math.round(item.Vega) + '</td>'
                        + '<td>' + item.unrealizedPNL + '</td>'
                        + '<td>' + item.realizedPNL + '</td>'
                        + '</tr>';
                })
            },
            error: function (a, b, c) {

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
                null, null, null, null, null, null, null, null, null, null,
                null, null, null, null
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