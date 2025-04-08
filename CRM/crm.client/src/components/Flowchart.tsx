import ReactFlow, { Node, Edge, MarkerType, Position } from "reactflow";
import { app } from "../app/MyApp";
import { useNavigate } from "react-router-dom";
import { AppModule } from "../app/AppModule";
import Container from "@mui/material/Container";

export function Flowchart({ visible }: { visible: boolean }) {
  // Route
  const navigate = useNavigate();

  if (!visible) return;

  // Labels
  const labels = app.getLabels(
    "contacts",
    "customers",
    "orders",
    "org",
    "products",
    "purchases",
    "reports",
    "simpleInventory",
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
        className: app.module[AppModule.Finance] ? undefined : "node-disabled",
        position: v ? { x: 90, y: 600 } : { x: 0, y: 0 }
      },
      {
        id: "contact",
        type: "input",
        data: { label: labels.contacts },
        position: v ? { x: 0, y: 0 } : { x: 250, y: 0 }
      },
      {
        id: "org",
        data: { label: labels.org },
        targetPosition: Position.Left,
        className: app.module[AppModule.Organization]
          ? undefined
          : "node-disabled",
        position: v ? { x: 180, y: 0 } : { x: 500, y: 0 }
      },
      {
        id: "supplier",
        data: { label: labels.suppliers },
        className: app.module[AppModule.Supplier] ? undefined : "node-disabled",
        position: v ? { x: 0, y: 200 } : { x: 0, y: 136 }
      },
      {
        id: "user",
        data: { label: labels.employees },
        className: app.module[AppModule.User] ? undefined : "node-disabled",
        position: v ? { x: 90, y: 140 } : { x: 250, y: 136 }
      },
      {
        id: "customer",
        data: { label: labels.customers },
        className: app.module[AppModule.Customer] ? undefined : "node-disabled",
        position: v ? { x: 180, y: 200 } : { x: 500, y: 136 }
      },
      {
        id: "product",
        data: { label: labels.products },
        className: app.module[AppModule.Product] ? undefined : "node-disabled",
        position: v ? { x: 90, y: 300 } : { x: 250, y: 200 }
      },
      {
        id: "order",
        data: { label: labels.orders },
        className: app.module[AppModule.Order] ? undefined : "node-disabled",
        position: v ? { x: 180, y: 500 } : { x: 500, y: 300 }
      },
      {
        id: "po",
        data: { label: labels.purchases },
        className: app.module[AppModule.PO] ? undefined : "node-disabled",
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

    const inventoryManagement = true;

    if (inventoryManagement) {
      // Inventory nodes
      nodes.push({
        id: "sim",
        data: { label: labels.simpleInventory },
        className: app.module[AppModule.Inventory]
          ? undefined
          : "node-disabled",
        position: v ? { x: 90, y: 400 } : { x: 250, y: 300 }
      });

      edges.push(
        {
          id: "poToSmi",
          source: "po",
          target: "sim",
          label: labels.stockIn,
          animated: true,
          markerEnd: {
            type: MarkerType.ArrowClosed
          }
        },
        {
          id: "smiToOrder",
          source: "sim",
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
          if (id) navigate(`./${id}/all`);
        }}
      ></ReactFlow>
    </Container>
  );
}
