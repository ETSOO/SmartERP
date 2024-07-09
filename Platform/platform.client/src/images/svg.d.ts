const ReactComponent: React.FunctionComponent<
  React.ComponentProps<"svg"> & { title?: string }
>;

declare module "*.svg?react" {
  export default ReactComponent;
}

declare module "*.svg?url&react" {
  export default ReactComponent;
}
