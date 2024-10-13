import { app } from "../app/MyApp";

function Home() {
  return <p>Home, {app.get("appCore")}</p>;
}

export default Home;
