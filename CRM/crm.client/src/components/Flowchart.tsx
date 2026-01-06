import ReactFlow, { Node, Edge, MarkerType, Position } from "reactflow";
import { app } from "../app/MyApp";
import { useNavigate } from "react-router-dom";
import Container from "@mui/material/Container";
import { Permissions } from "@etsoo/smarterp-crm";

export function Flowchart({ visible }: { visible: boolean }) {
  // Route
  const navigate = useNavigate();

  if (!visible) return;

  // Labels
  const labels = app.getLabels(
    "customers",
    "orders",
    "org",
    "products",
    "purchases",
    "reports",
    "simpleInventory",
    "stakeholders",
    "stockIn",
    "stockOut",
    "suppliers",
    "employees"
  );

  // Is vertical
  const v = app.smDown ?? false;

  // React flow nodes & edges
  const getLayout = () => {
    const nodes: Node[] = [
      {
        id: "report",
        data: { label: labels.reports },
        className: app.owns(Permissions.Finance.All)
          ? undefined
          : "node-disabled",
        position: v ? { x: 90, y: 600 } : { x: 0, y: 0 }
      },
      {
        id: "contact",
        type: "input",
        data: { label: labels.stakeholders },
        position: v ? { x: 0, y: 0 } : { x: 250, y: 0 }
      },
      {
        id: "org",
        data: { label: labels.org },
        targetPosition: Position.Left,
        className: app.owns(Permissions.Org.All) ? undefined : "node-disabled",
        position: v ? { x: 180, y: 0 } : { x: 500, y: 0 }
      },
      {
        id: "supplier",
        data: { label: labels.suppliers },
        className: app.owns(Permissions.Supplier.All)
          ? undefined
          : "node-disabled",
        position: v ? { x: 0, y: 200 } : { x: 0, y: 136 }
      },
      {
        id: "user",
        data: { label: labels.employees },
        className: app.owns(Permissions.User.All) ? undefined : "node-disabled",
        position: v ? { x: 90, y: 140 } : { x: 250, y: 136 }
      },
      {
        id: "customer",
        data: { label: labels.customers },
        className: app.owns(Permissions.Customer.All)
          ? undefined
          : "node-disabled",
        position: v ? { x: 180, y: 200 } : { x: 500, y: 136 }
      },
      {
        id: "product",
        data: { label: labels.products },
        className: app.owns(Permissions.Product.All)
          ? undefined
          : "node-disabled",
        position: v ? { x: 90, y: 300 } : { x: 250, y: 200 }
      },
      {
        id: "order",
        data: { label: labels.orders },
        className: app.owns(Permissions.Order.All)
          ? undefined
          : "node-disabled",
        position: v ? { x: 180, y: 500 } : { x: 500, y: 300 }
      },
      {
        id: "po",
        data: { label: labels.purchases },
        className: app.owns(Permissions.PO.All) ? undefined : "node-disabled",
        position: v ? { x: 0, y: 500 } : { x: 0, y: 300 }
      }
    ];

    const edges: Edge[] = [
      {
        id: "contactToOrg",
        source: "contact",
        target: "org",
        type: "step",
        markerEnd: {
          type: MarkerType.ArrowClosed
        }
      },
      {
        id: "contactToUser",
        source: "contact",
        target: "user",
        type: "step",
        markerEnd: {
          type: MarkerType.ArrowClosed
        }
      },
      {
        id: "contactToSupplier",
        source: "contact",
        target: "supplier",
        type: "step",
        markerEnd: {
          type: MarkerType.ArrowClosed
        }
      },
      {
        id: "contactToCustomer",
        source: "contact",
        target: "customer",
        type: "step",
        markerEnd: {
          type: MarkerType.ArrowClosed
        }
      },
      {
        id: "supplierToPo",
        source: "supplier",
        target: "po",
        animated: true,
        markerEnd: {
          type: MarkerType.ArrowClosed
        }
      },
      {
        id: "userToProduct",
        source: "user",
        target: "product",
        markerEnd: {
          type: MarkerType.ArrowClosed
        }
      },
      {
        id: "productToPo",
        source: "product",
        target: "po",
        animated: true,
        markerEnd: {
          type: MarkerType.ArrowClosed
        }
      },
      {
        id: "productToOrder",
        source: "product",
        target: "order",
        animated: true,
        markerEnd: {
          type: MarkerType.ArrowClosed
        }
      },
      {
        id: "customerToOrder",
        source: "customer",
        target: "order",
        animated: true,
        markerEnd: {
          type: MarkerType.ArrowClosed
        }
      }
    ];

    const inventoryManagement = app.userData?.system?.hasInventory ?? false;

    if (inventoryManagement) {
      // Inventory nodes
      nodes.push({
        id: "inventory",
        data: { label: labels.simpleInventory },
        className: app.owns(Permissions.Inventory.All)
          ? undefined
          : "node-disabled",
        position: v ? { x: 90, y: 400 } : { x: 250, y: 300 }
      });

      edges.push(
        {
          id: "poToInventory",
          source: "po",
          target: "inventory",
          label: labels.stockIn,
          animated: true,
          markerEnd: {
            type: MarkerType.ArrowClosed
          }
        },
        {
          id: "inventoryToOrder",
          source: "inventory",
          target: "order",
          label: labels.stockOut,
          animated: true,
          markerEnd: {
            type: MarkerType.ArrowClosed
          }
        }
      );
    }
    return { nodes, edges };
  };

  return (
    <Container maxWidth="md" sx={{ height: v ? "640px" : "408px" }}>
      <ReactFlow
        className="react-flow"
        {...getLayout()}
        fitView
        zoomOnScroll={false}
        zoomOnDoubleClick={false}
        panOnDrag={false}
        onClick={(event) => {
          const ele = event.target as HTMLElement;
          if (ele.classList.contains("node-disabled")) return;
          const id = ele.dataset["id"];
          if (id) navigate(`./${id}`);
        }}
      ></ReactFlow>
    </Container>
  );
}
