using Microsoft.AspNetCore.Mvc;
using PrecisionOMS.Core.Models;
using PrecisionOMS.Core.Services;

namespace PrecisionOMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductionOrdersController : ControllerBase
{
    private readonly IProductionOrderService _productionOrderService;
    private readonly ILogger<ProductionOrdersController> _logger;

    public ProductionOrdersController(
        IProductionOrderService productionOrderService,
        ILogger<ProductionOrdersController> logger)
    {
        _productionOrderService = productionOrderService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductionOrder>>> GetAllOrders()
    {
        try
        {
            var orders = await _productionOrderService.GetAllOrdersAsync();
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving production orders");
            return StatusCode(500, "Internal server error while retrieving production orders");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductionOrder>> GetOrder(int id)
    {
        try
        {
            var order = await _productionOrderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound($"Production order with ID {id} not found");
            }
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving production order {OrderId}", id);
            return StatusCode(500, "Internal server error while retrieving production order");
        }
    }

    [HttpGet("by-number/{orderNumber}")]
    public async Task<ActionResult<ProductionOrder>> GetOrderByNumber(string orderNumber)
    {
        try
        {
            var order = await _productionOrderService.GetOrderByNumberAsync(orderNumber);
            if (order == null)
            {
                return NotFound($"Production order with number {orderNumber} not found");
            }
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving production order {OrderNumber}", orderNumber);
            return StatusCode(500, "Internal server error while retrieving production order");
        }
    }

    [HttpGet("facility/{facilityId}")]
    public async Task<ActionResult<IEnumerable<ProductionOrder>>> GetOrdersByFacility(int facilityId)
    {
        try
        {
            var orders = await _productionOrderService.GetOrdersByFacilityAsync(facilityId);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders for facility {FacilityId}", facilityId);
            return StatusCode(500, "Internal server error while retrieving facility orders");
        }
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<ProductionOrder>>> GetOrdersByStatus(ProductionStatus status)
    {
        try
        {
            var orders = await _productionOrderService.GetOrdersByStatusAsync(status);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders with status {Status}", status);
            return StatusCode(500, "Internal server error while retrieving orders by status");
        }
    }

    [HttpGet("date-range")]
    public async Task<ActionResult<IEnumerable<ProductionOrder>>> GetOrdersByDateRange(
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate)
    {
        try
        {
            if (startDate > endDate)
            {
                return BadRequest("Start date cannot be later than end date");
            }

            var orders = await _productionOrderService.GetOrdersByDateRangeAsync(startDate, endDate);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders for date range {StartDate} to {EndDate}", startDate, endDate);
            return StatusCode(500, "Internal server error while retrieving orders by date range");
        }
    }

    [HttpPost]
    public async Task<ActionResult<ProductionOrder>> CreateOrder([FromBody] ProductionOrder order)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdOrder = await _productionOrderService.CreateOrderAsync(order);
            return CreatedAtAction(nameof(GetOrder), new { id = createdOrder.Id }, createdOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating production order");
            return StatusCode(500, "Internal server error while creating production order");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductionOrder>> UpdateOrder(int id, [FromBody] ProductionOrder order)
    {
        try
        {
            if (id != order.Id)
            {
                return BadRequest("Order ID mismatch");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingOrder = await _productionOrderService.GetOrderByIdAsync(id);
            if (existingOrder == null)
            {
                return NotFound($"Production order with ID {id} not found");
            }

            var updatedOrder = await _productionOrderService.UpdateOrderAsync(order);
            return Ok(updatedOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating production order {OrderId}", id);
            return StatusCode(500, "Internal server error while updating production order");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        try
        {
            var success = await _productionOrderService.DeleteOrderAsync(id);
            if (!success)
            {
                return NotFound($"Production order with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting production order {OrderId}", id);
            return StatusCode(500, "Internal server error while deleting production order");
        }
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] ProductionStatus status)
    {
        try
        {
            var success = await _productionOrderService.UpdateOrderStatusAsync(id, status);
            if (!success)
            {
                return NotFound($"Production order with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for production order {OrderId}", id);
            return StatusCode(500, "Internal server error while updating order status");
        }
    }

    [HttpGet("{id}/efficiency")]
    public async Task<ActionResult<decimal>> GetOrderEfficiency(int id)
    {
        try
        {
            var efficiency = await _productionOrderService.CalculateEfficiencyRatingAsync(id);
            return Ok(new { OrderId = id, EfficiencyRating = efficiency });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating efficiency for production order {OrderId}", id);
            return StatusCode(500, "Internal server error while calculating efficiency");
        }
    }

    [HttpGet("analytics")]
    public async Task<ActionResult<ProductionAnalytics>> GetProductionAnalytics(
        [FromQuery] int facilityId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            if (startDate > endDate)
            {
                return BadRequest("Start date cannot be later than end date");
            }

            var analytics = await _productionOrderService.GetProductionAnalyticsAsync(facilityId, startDate, endDate);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving production analytics for facility {FacilityId}", facilityId);
            return StatusCode(500, "Internal server error while retrieving analytics");
        }
    }

    [HttpGet("critical")]
    public async Task<ActionResult<IEnumerable<ProductionOrder>>> GetCriticalOrders()
    {
        try
        {
            var orders = await _productionOrderService.GetCriticalOrdersAsync();
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving critical orders");
            return StatusCode(500, "Internal server error while retrieving critical orders");
        }
    }

    [HttpGet("delayed")]
    public async Task<ActionResult<IEnumerable<ProductionOrder>>> GetDelayedOrders()
    {
        try
        {
            var orders = await _productionOrderService.GetDelayedOrdersAsync();
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving delayed orders");
            return StatusCode(500, "Internal server error while retrieving delayed orders");
        }
    }

    [HttpPost("facility/{facilityId}/optimize-schedule")]
    public async Task<IActionResult> OptimizeProductionSchedule(int facilityId)
    {
        try
        {
            var success = await _productionOrderService.OptimizeProductionScheduleAsync(facilityId);
            if (!success)
            {
                return BadRequest("Failed to optimize production schedule");
            }

            return Ok(new { Message = "Production schedule optimized successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing production schedule for facility {FacilityId}", facilityId);
            return StatusCode(500, "Internal server error while optimizing schedule");
        }
    }
} 